// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Class for posting activities securely to a bot using BotFramework HTTP protocol.
    /// </summary>
    /// <remarks>
    /// This class can be used to securely post activities to a bot using the Bot Framework HTTP protocol. There are 2 usage patterns:
    /// * Forwarding activity to a Skill (Bot => Bot as a Skill) which is done via PostActivityAsync(fromBotId, toBotId, endpoint, serviceUrl, activity);
    /// * Posting an activity to yourself (External service => Bot) which is done via PostActivityAsync(botId, endpoint, activity)
    /// The latter is used by external services such as webjobs that need to post activities to the bot using the bots own credentials.
    /// </remarks>
    public class BotFrameworkHttpClient : BotFrameworkClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpClient"/> class.
        /// </summary>
        /// <param name="httpClient">A <see cref="HttpClient"/>.</param>
        /// <param name="credentialProvider">An instance of <see cref="ICredentialProvider"/>.</param>
        /// <param name="channelProvider">An instance of <see cref="IChannelProvider"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        public BotFrameworkHttpClient(
            HttpClient httpClient,
            ICredentialProvider credentialProvider,
            IChannelProvider channelProvider = null,
            ILogger logger = null)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            CredentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            ChannelProvider = channelProvider;
            Logger = logger ?? NullLogger.Instance;
            ConnectorClient.AddDefaultRequestHeaders(HttpClient);
        }

        /// <summary>
        /// Gets the Cache for appCredentials to speed up token acquisition (a token is not requested unless is expired).
        /// AppCredentials are cached using appId + scope (this last parameter is only used if the app credentials are used to call a skill).
        /// </summary>
        /// <value>ConcurrentDictionary of <see cref="AppCredentials"/>.</value>
        protected static ConcurrentDictionary<string, AppCredentials> AppCredentialMapCache { get; } = new ConcurrentDictionary<string, AppCredentials>();

        /// <summary>
        /// Gets the channel provider for this adapter.
        /// </summary>
        /// <value>
        /// The channel provider for this adapter.
        /// </value>
        protected IChannelProvider ChannelProvider { get; }

        /// <summary>
        /// Gets the credential provider for this adapter.
        /// </summary>
        /// <value>
        /// The credential provider for this adapter.
        /// </value>
        protected ICredentialProvider CredentialProvider { get; }

        /// <summary>
        /// Gets the HttpClient for this adapter.
        /// </summary>
        /// <value>
        /// The HttpClient for this adapter.
        /// </value>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the logger for this adapter.
        /// </summary>
        /// <value>
        /// The logger for this adapter.
        /// </value>
        protected ILogger Logger { get; }

        /// <summary>
        /// Forwards an activity to a skill (bot).
        /// </summary>
        /// <remarks>NOTE: Forwarding an activity to a skill will flush UserState and ConversationState changes so that skill has accurate state.</remarks>
        /// <param name="fromBotId">The MicrosoftAppId of the bot sending the activity.</param>
        /// <param name="toBotId">The MicrosoftAppId of the bot receiving the activity.</param>
        /// <param name="toUrl">The URL of the bot receiving the activity.</param>
        /// <param name="serviceUrl">The callback Url for the skill host.</param>
        /// <param name="conversationId">A conversation ID to use for the conversation with the skill.</param>
        /// <param name="activity">activity to forward.</param>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Async task with optional invokeResponse.</returns>
        public override async Task<InvokeResponse> PostActivityAsync(string fromBotId, string toBotId, Uri toUrl, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            return await PostActivityAsync<object>(fromBotId, toBotId, toUrl, serviceUrl, conversationId, activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Forwards an activity to a skill (bot).
        /// </summary>
        /// <remarks>NOTE: Forwarding an activity to a skill will flush UserState and ConversationState changes so that skill has accurate state.</remarks>
        /// <typeparam name="T">The type of body in the InvokeResponse.</typeparam>
        /// <param name="fromBotId">The MicrosoftAppId of the bot sending the activity.</param>
        /// <param name="toBotId">The MicrosoftAppId of the bot receiving the activity.</param>
        /// <param name="toUrl">The URL of the bot receiving the activity.</param>
        /// <param name="serviceUrl">The callback Url for the skill host.</param>
        /// <param name="conversationId">A conversation ID to use for the conversation with the skill.</param>
        /// <param name="activity">activity to forward.</param>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Async task with optional invokeResponse<typeparamref name="T"/>.</returns>
        public override async Task<InvokeResponse<T>> PostActivityAsync<T>(string fromBotId, string toBotId, Uri toUrl, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            var appCredentials = await GetAppCredentialsAsync(fromBotId, toBotId).ConfigureAwait(false);
            if (appCredentials == null)
            {
                Logger.LogError("Unable to get appCredentials to connect to the skill");
                throw new InvalidOperationException("Unable to get appCredentials to connect to the skill");
            }

            // Get token for the skill call
            var token = appCredentials == MicrosoftAppCredentials.Empty ? null : await appCredentials.GetTokenAsync().ConfigureAwait(false);

            // Clone the activity so we can modify it before sending without impacting the original object.
            var activityClone = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
            activityClone.RelatesTo = new ConversationReference
            {
                ServiceUrl = activityClone.ServiceUrl,
                ActivityId = activityClone.Id,
                ChannelId = activityClone.ChannelId,
                Locale = activityClone.Locale,
                Conversation = new ConversationAccount
                {
                    Id = activityClone.Conversation.Id,
                    Name = activityClone.Conversation.Name,
                    ConversationType = activityClone.Conversation.ConversationType,
                    AadObjectId = activityClone.Conversation.AadObjectId,
                    IsGroup = activityClone.Conversation.IsGroup,
                    Properties = activityClone.Conversation.Properties,
                    Role = activityClone.Conversation.Role,
                    TenantId = activityClone.Conversation.TenantId,
                }
            };
            activityClone.Conversation.Id = conversationId;
            activityClone.ServiceUrl = serviceUrl.ToString();
            activityClone.Recipient ??= new ChannelAccount();
            activityClone.Recipient.Role = RoleTypes.Skill;

            return await SecurePostActivityAsync<T>(toUrl, activityClone, token, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Post Activity to the bot using the bot's credentials.
        /// </summary>
        /// <param name="botId">The MicrosoftAppId of the bot.</param>
        /// <param name="botEndpoint">The URL of the bot.</param>
        /// <param name="activity">activity to post.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>InvokeResponse.</returns>
        public virtual async Task<InvokeResponse> PostActivityAsync(string botId, Uri botEndpoint, Activity activity, CancellationToken cancellationToken = default)
        {
            return await PostActivityAsync<object>(botId, botEndpoint, activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Post Activity to the bot using the bot's credentials.
        /// </summary>
        /// <typeparam name="T">type of invokeResponse body.</typeparam>
        /// <param name="botId">The MicrosoftAppId of the bot.</param>
        /// <param name="botEndpoint">The URL of the bot.</param>
        /// <param name="activity">activity to post.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>InvokeResponse<typeparamref name="T"/>.</returns>
        public virtual async Task<InvokeResponse<T>> PostActivityAsync<T>(string botId, Uri botEndpoint, Activity activity, CancellationToken cancellationToken = default)
        {
            // From BotId => BotId
            var appCredentials = await GetAppCredentialsAsync(botId, botId).ConfigureAwait(false);
            if (appCredentials == null)
            {
                throw new InvalidOperationException($"Unable to get appCredentials for the bot Id={botId}");
            }

            // Get token for the bot to call itself
            var token = appCredentials == MicrosoftAppCredentials.Empty ? null : await appCredentials.GetTokenAsync().ConfigureAwait(false);

            // post the activity to the url using the bot's credentials.
            Logger.LogInformation($"Posting activity. ActivityId: {activity.Id} from BotId: {botId}");
            return await SecurePostActivityAsync<T>(botEndpoint, activity, token, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Logic to build an <see cref="AppCredentials"/> object to be used to acquire tokens
        /// for this HttpClient.
        /// </summary>
        /// <param name="appId">The application id.</param>
        /// <param name="oAuthScope">The optional OAuth scope.</param>
        /// <returns>The app credentials to be used to acquire tokens.</returns>
        protected virtual async Task<AppCredentials> BuildCredentialsAsync(string appId, string oAuthScope = null)
        {
            var appPassword = await CredentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);
            return ChannelProvider != null && ChannelProvider.IsGovernment() ? new MicrosoftGovernmentAppCredentials(appId, appPassword, HttpClient, Logger, oAuthScope) : new MicrosoftAppCredentials(appId, appPassword, HttpClient, Logger, oAuthScope);
        }

        private static T GetBodyContent<T>(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (JsonException)
            {
                // This will only happen when the skill didn't return valid json in the content (e.g. when the status code is 500 or there's a bug in the skill)
                return default;
            }
        }

        private async Task<InvokeResponse<T>> SecurePostActivityAsync<T>(Uri toUrl, Activity activity, string token, CancellationToken cancellationToken)
        {
            using (var jsonContent = new StringContent(JsonConvert.SerializeObject(activity, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json"))
            {
                using (var httpRequestMessage = new HttpRequestMessage())
                {
                    httpRequestMessage.Method = HttpMethod.Post;
                    httpRequestMessage.RequestUri = toUrl;
                    if (token != null)
                    {
                        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    httpRequestMessage.Headers.Add(ConversationConstants.ConversationIdHttpHeaderName, activity.Conversation.Id);

                    httpRequestMessage.Content = jsonContent;
                    using (var response = await HttpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false))
                    {
                        var content = response.Content != null ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) : null;
                        return new InvokeResponse<T>
                        {
                            Status = (int)response.StatusCode,
                            Body = content?.Length > 0 ? GetBodyContent<T>(content) : default
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Gets the application credentials. App Credentials are cached so as to ensure we are not refreshing
        /// token every time.
        /// </summary>
        /// <param name="appId">The application identifier (AAD Id for the bot).</param>
        /// <param name="oAuthScope">The scope for the token, skills will use the Skill App Id. </param>
        /// <returns>App credentials.</returns>
        private async Task<AppCredentials> GetAppCredentialsAsync(string appId, string oAuthScope = null)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return MicrosoftAppCredentials.Empty;
            }

            // If the credentials are in the cache, retrieve them from there
            var cacheKey = $"{appId}{oAuthScope}";
            if (AppCredentialMapCache.TryGetValue(cacheKey, out var appCredentials))
            {
                return appCredentials;
            }

            // Credentials not found in cache, build them
            appCredentials = await BuildCredentialsAsync(appId, oAuthScope).ConfigureAwait(false);

            // Cache the credentials for later use
            AppCredentialMapCache[cacheKey] = appCredentials;
            return appCredentials;
        }
    }
}
