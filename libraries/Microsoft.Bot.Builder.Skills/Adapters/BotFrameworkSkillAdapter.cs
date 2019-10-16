// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills.BotFramework;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest.TransientFaultHandling;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Skills.Adapters
{
    /// <summary>
    /// A skill adapter that can connect a bot to a another bot as a skill.
    /// </summary>
    /// <remarks>The skill adapter encapsulates authentication processes  routing
    /// activities from a bot to another bot utilize as a skill. 
    /// <para>Use <see cref="Use(IMiddleware)"/> to add <see cref="IMiddleware"/> objects
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
    public class BotFrameworkSkillAdapter : SkillAdapter
    {
        internal const string BotIdentityKey = "BotIdentity";
        private const string InvokeResponseKey = "BotFrameworkAdapter.InvokeResponse";

        private static readonly HttpClient _defaultHttpClient = new HttpClient();
        private readonly ICredentialProvider _credentialProvider;
        private readonly AppCredentials _appCredentials;
        private readonly IChannelProvider _channelProvider;
        private readonly HttpClient _httpClient;
        private readonly RetryPolicy _connectorClientRetryPolicy;
        private readonly ILogger _logger;

        // Cache for appCredentials to speed up token acquisition (a token is not requested unless is expired)
        // AppCredentials are cached using appId + skillId (this last parameter is only used if the app credentials are used to call a skill)
        private readonly ConcurrentDictionary<string, AppCredentials> _appCredentialMap = new ConcurrentDictionary<string, AppCredentials>();

        // There is a significant boost in throughput if we reuse a connectorClient
        // _connectorClients is a cache using [serviceUrl + appId].
        private readonly ConcurrentDictionary<string, ConnectorClient> _connectorClients = new ConcurrentDictionary<string, ConnectorClient>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkSkillAdapter"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="adapter">adapter that this skillAdapter is bound to</param>
        /// <param name="bot">bot callback to use in the turn context</param>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <param name="configuration">configuration</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="credentialProvider"/> is <c>null</c>.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the <see cref="Use(IMiddleware)"/> method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkSkillAdapter(
            BotAdapter adapter,
            IBot bot,
            ICredentialProvider credentialProvider,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            ILogger logger = null,
            IConfiguration configuration = null)
            : base(adapter, bot, logger)
        {
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _channelProvider = channelProvider;
            _httpClient = customHttpClient ?? _defaultHttpClient;
            _connectorClientRetryPolicy = connectorClientRetryPolicy;
            _logger = logger ?? NullLogger.Instance;

            LoadFromConfiguration(configuration);

            // DefaultRequestHeaders are not thread safe so set them up here because this adapter should be a singleton.
            ConnectorClient.AddDefaultRequestHeaders(_httpClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkSkillAdapter"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="adapter">adapter that this skillAdapter is bound to</param>
        /// <param name="bot">bot callback </param>
        /// <param name="credentials">The credentials to be used for token acquisition.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <param name="configuration">configuration</param>
        /// <exception cref="ArgumentNullException">throw ArgumentNullException</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the <see cref="Use(IMiddleware)"/> method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkSkillAdapter(
            BotAdapter adapter,
            IBot bot,
            AppCredentials credentials,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            ILogger logger = null,
            IConfiguration configuration = null)
            : base(adapter, bot, logger)
        {
            _appCredentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _credentialProvider = new SimpleCredentialProvider(credentials.MicrosoftAppId, string.Empty);
            _channelProvider = channelProvider;
            _httpClient = customHttpClient ?? _defaultHttpClient;
            _connectorClientRetryPolicy = connectorClientRetryPolicy;
            _logger = logger ?? NullLogger.Instance;

            LoadFromConfiguration(configuration);

            // DefaultRequestHeaders are not thread safe so set them up here because this adapter should be a singleton.
            ConnectorClient.AddDefaultRequestHeaders(_httpClient);
        }

        public List<BotFrameworkSkill> Skills { get; private set; } = new List<BotFrameworkSkill>();

        /// <summary>
        /// Gets or sets the /v3/conversations endpoint that will handle responses from the skill..
        /// </summary>
        /// <value>
        /// The callback URL that will be used by the skills to communicate back to the bot.
        /// </value>
        public string SkillsCallbackEndpoint { get; set; }

        /// <summary>
        /// Forward an activity to a skill(bot).
        /// </summary>
        /// <remarks>NOTE: Forwarding an activity to a skill will flush UserState and ConversationState changes so that skill has accurate state.</remarks>
        /// <param name="turnContext">turnContext.</param>
        /// <param name="skillId">skillId of the skill to forward the activity to.</param>
        /// <param name="activity">activity to forward.</param>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Async task with optional invokeResponse.</returns>
        public override async Task<InvokeResponse> ForwardActivityAsync(ITurnContext turnContext, string skillId, Activity activity, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Received request to forward activity to skill id {skillId}.");

            // Get the skill information for the skillId
            var skill = Skills.FirstOrDefault(s => s.Id == skillId);
            if (skill == null)
            {
                throw new ArgumentException($"Skill:{skillId} isn't a registered skill");
            }

            // Pull the current claims identity from TurnState (it is stored there on the way in).
            var identity = (ClaimsIdentity)turnContext.TurnState.Get<IIdentity>(BotIdentityKey);
            if (identity.AuthenticationType.Equals("anonymous", StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO: validate that we won't support anonymous with skills (sort of like OAuth). Gabo
                throw new NotSupportedException("Anonymous calls are not supported for skills, please ensure your bot is configured with a MicrosoftAppId and Password).");
            }

            // Get current Bot ID from the identity audience claim
            var botAppId = identity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim)?.Value;
            if (string.IsNullOrWhiteSpace(botAppId))
            {
                throw new InvalidOperationException("Unable to get the audience from the current request identity");
            }

            var appCredentials = await GetAppCredentialsAsync(botAppId, skill.AppId, cancellationToken).ConfigureAwait(false);
            if (appCredentials == null)
            {
                throw new InvalidOperationException("Unable to get appcredentials to connect to the skill");
            }

            // Get token for the skill call
            var token = await appCredentials.GetTokenAsync().ConfigureAwait(false);

            // POST to skill 
            using (var client = new HttpClient())
            {
                // TODO use SkillConversation class here instead of hard coded encoding...
                // Encode original bot service URL and ConversationId in the new conversation ID so we can unpack it later.
                // var skillConversation = new SkillConversation() { ServiceUrl = activity.ServiceUrl, ConversationId = activity.Conversation.Id };
                // activity.Conversation.Id = skillConversation.GetSkillConversationId()
                activity.Conversation.Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new string[]
                {
                    activity.Conversation.Id,
                    activity.ServiceUrl,
                })));
                activity.ServiceUrl = SkillsCallbackEndpoint;
                activity.Recipient.Properties["skillId"] = skill.Id;
                var jsonContent = new StringContent(JsonConvert.SerializeObject(activity, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.PostAsync($"{skill.SkillEndpoint}", jsonContent, cancellationToken).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (content.Length > 0)
                {
                    return JsonConvert.DeserializeObject<InvokeResponse>(content);
                }
            }

            return null;
        }

        /// <summary>
        /// Creates the connector client asynchronous.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="claimsIdentity">The claims identity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ConnectorClient instance.</returns>
        /// <exception cref="NotSupportedException">ClaimsIdentity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.</exception>
        private async Task<IConnectorClient> CreateConnectorClientAsync(string serviceUrl, ClaimsIdentity claimsIdentity, CancellationToken cancellationToken)
        {
            if (claimsIdentity == null)
            {
                throw new NotSupportedException("ClaimsIdentity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.");
            }

            // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For
            // unauthenticated requests we have anonymous identity provided auth is disabled.
            // For Activities coming from Emulator AppId claim contains the Bot's AAD AppId.
            var botAppIdClaim = claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim);
            if (botAppIdClaim == null)
            {
                botAppIdClaim = claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AppIdClaim);
            }

            // For anonymous requests (requests with no header) appId is not set in claims.
            AppCredentials appCredentials = null;
            if (botAppIdClaim != null)
            {
                var botId = botAppIdClaim.Value;
                string scope = null;
                if (SkillValidation.IsSkillClaim(claimsIdentity.Claims))
                {
                    // The skill connector has the target skill in the OAuthScope.
                    scope = JwtTokenValidation.GetAppId(claimsIdentity.Claims);
                }

                appCredentials = await GetAppCredentialsAsync(botId, scope, cancellationToken).ConfigureAwait(false);
            }

            return CreateConnectorClient(serviceUrl, appCredentials);
        }

        /// <summary>
        /// Creates the connector client.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="appCredentials">The application credentials for the bot.</param>
        /// <returns>Connector client instance.</returns>
        private IConnectorClient CreateConnectorClient(string serviceUrl, AppCredentials appCredentials = null)
        {
            var clientKey = $"{serviceUrl}{appCredentials?.MicrosoftAppId ?? string.Empty}";

            return _connectorClients.GetOrAdd(clientKey, (key) =>
            {
                ConnectorClient connectorClient;
                if (appCredentials != null)
                {
                    connectorClient = new ConnectorClient(new Uri(serviceUrl), appCredentials, customHttpClient: _httpClient);
                }
                else
                {
                    var emptyCredentials = (_channelProvider != null && _channelProvider.IsGovernment()) ?
                        MicrosoftGovernmentAppCredentials.Empty :
                        MicrosoftAppCredentials.Empty;
                    connectorClient = new ConnectorClient(new Uri(serviceUrl), emptyCredentials, customHttpClient: _httpClient);
                }

                if (_connectorClientRetryPolicy != null)
                {
                    connectorClient.SetRetryPolicy(_connectorClientRetryPolicy);
                }

                return connectorClient;
            });
        }

        /// <summary>
        /// Gets the application credentials. App Credentials are cached so as to ensure we are not refreshing
        /// token every time.
        /// </summary>
        /// <param name="appId">The application identifier (AAD Id for the bot).</param>
        /// <param name="oAuthScope">The scope for the token, skills will use the Skill App Id. </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>App credentials.</returns>
        private async Task<AppCredentials> GetAppCredentialsAsync(string appId, string oAuthScope = null, CancellationToken cancellationToken = default)
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
            appCredentials = _channelProvider != null && _channelProvider.IsGovernment() ?
                new MicrosoftGovernmentAppCredentials(appId, appPassword, _httpClient, this._logger) :
                new MicrosoftAppCredentials(appId, appPassword, _httpClient, this._logger, oAuthScope);

            // Cache the credentials for later use
            _appCredentialMap[cacheKey] = appCredentials;
            return appCredentials;
        }

        private void LoadFromConfiguration(IConfiguration configuration)
        {
            var section = configuration?.GetSection($"BotFrameworkSkills");
            if (section != null)
            {
                var skills = section?.Get<BotFrameworkSkill[]>();
                if (skills != null)
                {
                    this.Skills.AddRange(skills);
                }
            }

            this.SkillsCallbackEndpoint = configuration?.GetValue<string>("SkillsCallbackEndpoint");
        }
    }
}
