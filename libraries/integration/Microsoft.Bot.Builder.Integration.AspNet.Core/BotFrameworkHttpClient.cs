// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public class BotFrameworkHttpClient
    {
        // Cache for appCredentials to speed up token acquisition (a token is not requested unless is expired)
        // AppCredentials are cached using appId + scope (this last parameter is only used if the app credentials are used to call a skill)
        private static readonly ConcurrentDictionary<string, AppCredentials> _appCredentialMapCache = new ConcurrentDictionary<string, AppCredentials>();
        private readonly IChannelProvider _channelProvider;
        private readonly ICredentialProvider _credentialProvider;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public BotFrameworkHttpClient(
            HttpClient httpClient,
            ICredentialProvider credentialProvider,
            IChannelProvider channelProvider = null,
            ILogger logger = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _channelProvider = channelProvider;
            _logger = logger ?? NullLogger.Instance;
            ConnectorClient.AddDefaultRequestHeaders(_httpClient);
        }

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
        public async Task<InvokeResponse> PostActivityAsync(string fromBotId, string toBotId, Uri toUrl, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken)
        {
            var appCredentials = await GetAppCredentialsAsync(fromBotId, toBotId).ConfigureAwait(false);
            if (appCredentials == null)
            {
                throw new InvalidOperationException("Unable to get appCredentials to connect to the skill");
            }

            // Get token for the skill call
            var token = await appCredentials.GetTokenAsync().ConfigureAwait(false);

            // Capture current activity settings before changing them.
            // TODO: DO we need to set the activity ID? (events that are created manually don't have it).
            var originalConversationId = activity.Conversation.Id;
            var originalServiceUrl = activity.ServiceUrl;
            try
            {
                activity.Conversation.Id = conversationId;
                activity.ServiceUrl = serviceUrl.ToString();

                using (var jsonContent = new StringContent(JsonConvert.SerializeObject(activity, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json"))
                {
                    using (var httpRequestMessage = new HttpRequestMessage())
                    {
                        httpRequestMessage.Method = HttpMethod.Post;
                        httpRequestMessage.RequestUri = toUrl;
                        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        httpRequestMessage.Content = jsonContent;

                        var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);

                        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        return new InvokeResponse
                        {
                            Status = (int)response.StatusCode,
                            Body = content.Length > 0 ? JsonConvert.DeserializeObject(content) : null
                        };
                    }
                }
            }
            finally
            {
                // Restore activity properties.
                activity.Conversation.Id = originalConversationId;
                activity.ServiceUrl = originalServiceUrl;
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
            if (appId == null)
            {
                return MicrosoftAppCredentials.Empty;
            }

            var cacheKey = $"{appId}{oAuthScope}";
            if (_appCredentialMapCache.TryGetValue(cacheKey, out var appCredentials))
            {
                return appCredentials;
            }

            // NOTE: we can't do async operations inside of a AddOrUpdate, so we split access pattern
            var appPassword = await _credentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);
            appCredentials = _channelProvider != null && _channelProvider.IsGovernment() ? new MicrosoftGovernmentAppCredentials(appId, appPassword, _httpClient, _logger) : new MicrosoftAppCredentials(appId, appPassword, _httpClient, _logger, oAuthScope);

            // Cache the credentials for later use
            _appCredentialMapCache[cacheKey] = appCredentials;
            return appCredentials;
        }
    }
}
