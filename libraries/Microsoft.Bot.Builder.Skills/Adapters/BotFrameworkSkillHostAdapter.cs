// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest.TransientFaultHandling;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Skills.Adapters
{
    /// <summary>
    /// A skill adapter that can connect a bot to a another bot as a skill.
    /// </summary>
    /// <remarks>The skill adapter encapsulates authentication processes  routing
    /// activities from a bot to another bot utilize as a skill. 
    /// <para>Use Use(<see cref="IMiddleware"/>) to add <see cref="IMiddleware"/> objects
    /// to your adapter’s middleware collection. The adapter processes and directs
    /// incoming activities in through the bot middleware pipeline to your bot’s logic
    /// and then back out again. As each activity flows in and out of the bot, each piece
    /// of middleware can inspect or act upon the activity, both before and after the bot
    /// logic runs.</para>
    /// </remarks>
    /// <seealso cref="ITurnContext"/>
    /// <seealso cref="IActivity"/>
    /// <seealso cref="IBot"/>
    /// <seealso cref="IMiddleware"/>
    public class BotFrameworkSkillHostAdapter : SkillHostAdapter
    {
        internal const string BotIdentityKey = "BotIdentity";

        private static readonly HttpClient _defaultHttpClient = new HttpClient();

        // Cache for appCredentials to speed up token acquisition (a token is not requested unless is expired)
        // AppCredentials are cached using appId + skillId (this last parameter is only used if the app credentials are used to call a skill)
        private readonly ConcurrentDictionary<string, AppCredentials> _appCredentialMap = new ConcurrentDictionary<string, AppCredentials>();
        private readonly AppCredentials _appCredentials;
        private readonly AuthenticationConfiguration _authConfiguration;
        private readonly IChannelProvider _channelProvider;
        private readonly RetryPolicy _connectorClientRetryPolicy;
        private readonly ICredentialProvider _credentialProvider;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkSkillHostAdapter"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="adapter">adapter that this skillAdapter is bound to.</param>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the Use(<see cref="IMiddleware"/>) method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkSkillHostAdapter(
            BotAdapter adapter,
            ICredentialProvider credentialProvider,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            ILogger logger = null)
            : this(adapter, credentialProvider, new AuthenticationConfiguration(), channelProvider, connectorClientRetryPolicy, customHttpClient, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkSkillHostAdapter"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="adapter">adapter that this skillAdapter is bound to.</param>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <exception cref="ArgumentNullException">throw ArgumentNullException.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the Use(<see cref="IMiddleware"/>) method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkSkillHostAdapter(
            BotAdapter adapter,
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfig,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            ILogger logger = null)
            : base(adapter, logger)
        {
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _channelProvider = channelProvider;
            _httpClient = customHttpClient ?? _defaultHttpClient;
            _connectorClientRetryPolicy = connectorClientRetryPolicy;
            _logger = logger ?? NullLogger.Instance;
            _authConfiguration = authConfig ?? throw new ArgumentNullException(nameof(authConfig));

            // DefaultRequestHeaders are not thread safe so set them up here because this adapter should be a singleton.
            ConnectorClient.AddDefaultRequestHeaders(_httpClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkSkillHostAdapter"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="adapter">adapter that this skillAdapter is bound to.</param>
        /// <param name="credentials">The credentials to be used for token acquisition.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <exception cref="ArgumentNullException">throw ArgumentNullException.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the Use(<see cref="IMiddleware"/>) method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkSkillHostAdapter(
            BotAdapter adapter,
            AppCredentials credentials,
            AuthenticationConfiguration authConfig,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            ILogger logger = null)
            : base(adapter, logger)
        {
            _appCredentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _channelProvider = channelProvider;
            _httpClient = customHttpClient ?? _defaultHttpClient;
            _connectorClientRetryPolicy = connectorClientRetryPolicy;
            _logger = logger ?? NullLogger.Instance;
            _authConfiguration = authConfig ?? throw new ArgumentNullException(nameof(authConfig));

            // DefaultRequestHeaders are not thread safe so set them up here because this adapter should be a singleton.
            ConnectorClient.AddDefaultRequestHeaders(_httpClient);
        }

        /// <summary>
        /// Forwards an activity to a skill (bot).
        /// </summary>
        /// <remarks>NOTE: Forwarding an activity to a skill will flush UserState and ConversationState changes so that skill has accurate state.</remarks>
        /// <param name="turnContext">turnContext.</param>
        /// <param name="skill">A <see cref="BotFrameworkSkill"/> instance with the skill information.</param>
        /// <param name="skillHostEndpoint">The callback Url for the skill host.</param>
        /// <param name="activity">activity to forward.</param>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Async task with optional invokeResponse.</returns>
        public override async Task<InvokeResponse> ForwardActivityAsync(ITurnContext turnContext, BotFrameworkSkill skill, Uri skillHostEndpoint, Activity activity, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Received request to forward activity to skill id {skill.Id}.");

            // Pull the current claims identity from TurnState (it is stored there on the way in).
            var identity = (ClaimsIdentity)turnContext.TurnState.Get<IIdentity>(BotIdentityKey);
            if (identity.AuthenticationType.Equals("anonymous", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new NotSupportedException("Anonymous calls are not supported for skills, please ensure your bot is configured with a MicrosoftAppId and Password).");
            }

            // Get current Bot ID from the identity audience claim
            var botAppId = identity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim)?.Value;
            if (string.IsNullOrWhiteSpace(botAppId))
            {
                throw new InvalidOperationException("Unable to get the audience from the current request identity");
            }

            var appCredentials = await GetAppCredentialsAsync(botAppId, skill.AppId).ConfigureAwait(false);
            if (appCredentials == null)
            {
                throw new InvalidOperationException("Unable to get appCredentials to connect to the skill");
            }

            // Get token for the skill call
            var token = await appCredentials.GetTokenAsync().ConfigureAwait(false);

            // POST to skill 
            using (var client = new HttpClient())
            {
                // Create a deep clone of the activity so we can update it without impacting the original activity.
                var activityClone = JObject.FromObject(activity).ToObject<Activity>();

                // TODO use SkillConversation class here instead of hard coded encoding...
                // Encode original bot service URL and ConversationId in the new conversation ID so we can unpack it later.
                // var skillConversation = new SkillConversation() { ServiceUrl = activity.ServiceUrl, ConversationId = activity.Conversation.Id };
                // activity.Conversation.Id = skillConversation.GetSkillConversationId()
                activityClone.Conversation.Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new[]
                {
                    activityClone.Conversation.Id,
                    activityClone.ServiceUrl
                })));
                activityClone.ServiceUrl = skillHostEndpoint.ToString();
                activityClone.Recipient.Properties["skillId"] = skill.Id;
                using (var jsonContent = new StringContent(JsonConvert.SerializeObject(activityClone, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json"))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.PostAsync($"{skill.SkillEndpoint}", jsonContent, cancellationToken).ConfigureAwait(false);
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (content.Length > 0)
                    {
                        return new InvokeResponse()
                        {
                            Status = (int)response.StatusCode,
                            Body = JsonConvert.DeserializeObject(content)
                        };
                    }
                }
            }

            return null;
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
            if (_appCredentialMap.TryGetValue(cacheKey, out var appCredentials))
            {
                return appCredentials;
            }

            // If app credentials were provided, use them as they are the preferred choice moving forward
            if (_appCredentials != null)
            {
                // Cache the credentials for later use
                _appCredentialMap[cacheKey] = _appCredentials;
                return _appCredentials;
            }

            // NOTE: we can't do async operations inside of a AddOrUpdate, so we split access pattern
            var appPassword = await _credentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);
            appCredentials = _channelProvider != null && _channelProvider.IsGovernment() ? new MicrosoftGovernmentAppCredentials(appId, appPassword, _httpClient, _logger) : new MicrosoftAppCredentials(appId, appPassword, _httpClient, _logger, oAuthScope);

            // Cache the credentials for later use
            _appCredentialMap[cacheKey] = appCredentials;
            return appCredentials;
        }
    }
}
