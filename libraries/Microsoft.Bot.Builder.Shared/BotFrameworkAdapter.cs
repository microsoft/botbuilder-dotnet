// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Rest.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters
{
    public partial class BotFrameworkAdapter : BotAdapter
    {
        private readonly ICredentialProvider _credentialProvider;
        private readonly HttpClient _httpClient;
        private readonly RetryPolicy _connectorClientRetryPolicy;
        private Dictionary<string, MicrosoftAppCredentials> _appCredentialMap = new Dictionary<string, MicrosoftAppCredentials>();

        /// <summary>
        /// Call context storage to propagate values throught the request.
        /// </summary>
        private static AsyncLocal<BotFrameworkAuthenticationContext> asyncLocal = new AsyncLocal<BotFrameworkAuthenticationContext>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkAdapter"/> class.
        /// This constructor is not supported for .NetCore apps.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="middleware">The middleware to use. Use <see cref="MiddlewareSet" class to register multiple middlewares together./></param>
        public BotFrameworkAdapter(
            ICredentialProvider credentialProvider,
            RetryPolicy connectorClientRetryPolicy = null, 
            HttpClient httpClient = null, 
            IMiddleware middleware = null)
        {
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _httpClient = httpClient ?? new HttpClient();
            _connectorClientRetryPolicy = connectorClientRetryPolicy;

            if (middleware != null)
            {
                this.Use(middleware);
            }
        }

        public override async Task ContinueConversation(ConversationReference reference, Func<ITurnContext, Task> callback)
        {
            BotFrameworkAuthenticationContext authenticationContext = asyncLocal.Value;

            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            var context = new TurnContext(this, reference.GetPostToBotMessage());

            context.Services.Add<IIdentity>("BotIdentity", authenticationContext.ClaimsIdentity);
            var connectorClient = await this.CreateConnectorClientAsync(reference.ServiceUrl, authenticationContext.ClaimsIdentity).ConfigureAwait(false);
            context.Services.Add<IConnectorClient>(connectorClient);
            await RunPipeline(context, callback).ConfigureAwait(false);
        }

        public new BotFrameworkAdapter Use(IMiddleware middleware)
        {
            base._middlewareSet.Use(middleware);
            return this;
        }

        public override async Task ProcessActivity(Activity activity, Func<ITurnContext, Task> callback, CancellationToken cancelToken = default(CancellationToken))
        {
            BotAssert.ActivityNotNull(activity);
            var claimsIdentity = await JwtTokenValidation.AuthenticateRequest(activity, this.GetAuthenticationHeader(), _credentialProvider, _httpClient).ConfigureAwait(false);

            var authenticationContext = new BotFrameworkAuthenticationContext
            {
                ClaimsIdentity = claimsIdentity,
                ServiceUrl = activity.ServiceUrl
            };

            asyncLocal.Value = authenticationContext;

            var context = new TurnContext(this, activity);
            context.Services.Add<IIdentity>("BotIdentity", claimsIdentity);
            var connectorClient = await this.CreateConnectorClientAsync(activity.ServiceUrl, claimsIdentity).ConfigureAwait(false);
            context.Services.Add<IConnectorClient>(connectorClient);

            cancelToken.ThrowIfCancellationRequested();
            await base.RunPipeline(context, callback).ConfigureAwait(false);
        }

        public override async Task<ResourceResponse[]> SendActivities(ITurnContext context, Activity[] activities)
        {
            List<ResourceResponse> responses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                ResourceResponse response;

                if (activity.Type == ActivityTypesEx.Delay)
                {
                    // The Activity Schema doesn't have a delay type build in, so it's simulated
                    // here in the Bot. This matches the behavior in the Node connector. 
                    int delayMs = (int)activity.Value;
                    await Task.Delay(delayMs).ConfigureAwait(false);

                    // In the case of a Delay, just create a fake one. Match the incoming activityId if it's there. 
                    response = new ResourceResponse(activity.Id ?? string.Empty);
                }
                else
                {
                    var connectorClient = context.Services.Get<IConnectorClient>();
                    response = await connectorClient.Conversations.SendToConversationAsync(activity).ConfigureAwait(false);
                }

                // Collect all the responses that come from the service. 
                responses.Add(response);
            }

            return responses.ToArray();
        }

        public override async Task<ResourceResponse> UpdateActivity(ITurnContext context, Activity activity)
        {
            var connectorClient = context.Services.Get<IConnectorClient>();
            return await connectorClient.Conversations.UpdateActivityAsync(activity).ConfigureAwait(false);
        }

        public override async Task DeleteActivity(ITurnContext context, ConversationReference reference)
        {
            var connectorClient = context.Services.Get<IConnectorClient>();
            await connectorClient.Conversations.DeleteActivityAsync(reference.Conversation.Id, reference.ActivityId).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a conversation on the Bot Framework for the given serviceUrl
        /// </summary>
        /// <param name="serviceUrl">serviceUrl you want to use</param>
        /// <param name="credentials">credentials</param>
        /// <param name="conversationParameters">arguments for the conversation you want to create</param>
        /// <param name="callback">callback which will have the context.Request.Conversation.Id in it</param>
        /// <returns></returns>
        public override async Task CreateConversation(string channelId, ConversationParameters conversationParameters, Func<ITurnContext, Task> callback)
        {
            BotFrameworkAuthenticationContext authenticationContext = asyncLocal.Value;

            var connectorClient = await this.CreateConnectorClientAsync(authenticationContext.ServiceUrl, authenticationContext.ClaimsIdentity).ConfigureAwait(false);

            var result = await connectorClient.Conversations.CreateConversationAsync(conversationParameters).ConfigureAwait(false);

            // create conversation Update to represent the result of creating the conversation
            var conversationUpdate = Activity.CreateConversationUpdateActivity();
            conversationUpdate.ChannelId = channelId;
            conversationUpdate.TopicName = conversationParameters.TopicName;
            conversationUpdate.ServiceUrl = authenticationContext.ServiceUrl;
            conversationUpdate.MembersAdded = conversationParameters.Members;
            conversationUpdate.Id = result.ActivityId ?? Guid.NewGuid().ToString("n");
            conversationUpdate.Conversation = new ConversationAccount(id: result.Id);
            conversationUpdate.Recipient = conversationParameters.Bot;

            TurnContext context = new TurnContext(this, (Activity)conversationUpdate);
            await this.RunPipeline(context, callback).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the request authentication context.
        /// </summary>
        private static void SetRequestAuthenticationContext(BotFrameworkAuthenticationContext authenticationContext)
        {
            asyncLocal.Value = authenticationContext;
        }

        /// <summary>
        /// Gets the request context.
        /// </summary>
        /// <returns>Request context.</returns>
        private static BotFrameworkAuthenticationContext GetBotFrameworkAuthenticationContext()
        {
            try
            {
                return asyncLocal.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates the connector client asynchronous.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="claimsIdentity">The claims identity.</param>
        /// <returns>ConnectorClient instance.</returns>
        /// <exception cref="NotSupportedException">ClaimsIdemtity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.</exception>
        private async Task<IConnectorClient> CreateConnectorClientAsync(string serviceUrl, ClaimsIdentity claimsIdentity)
        {
            if (claimsIdentity == null)
            {
                throw new NotSupportedException("ClaimsIdemtity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.");
            }

            // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For 
            // unauthenticated requests we have anonymouse identity provided auth is disabled.
            var botAppIdClaim = (claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim)
                ??
                // For Activities coming from Emulator AppId claim contains the Bot's AAD AppId.
                claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AppIdClaim));

            // For anonymous requests (requests with no header) appId is not set in claims.
            if (botAppIdClaim != null)
            {
                string botId = botAppIdClaim.Value;
                var appCredentials = await this.GetAppCredentialsAsync(botId).ConfigureAwait(false);
                return this.CreateConnectorClient(serviceUrl, appCredentials);
            }
            else
            {
                return this.CreateConnectorClient(serviceUrl);
            }
        }

        /// <summary>
        /// Creates the connector client.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="appCredentials">The application credentials.</param>
        /// <returns>Connector client instance.</returns>
        private IConnectorClient CreateConnectorClient(string serviceUrl, MicrosoftAppCredentials appCredentials = null)
        {
            ConnectorClient connectorClient;
            if (appCredentials != null)
            {
                connectorClient = new ConnectorClient(new Uri(serviceUrl), appCredentials);
            }
            else
            {
                connectorClient = new ConnectorClient(new Uri(serviceUrl));
            }

            if (this._connectorClientRetryPolicy != null)
            {
                connectorClient.SetRetryPolicy(this._connectorClientRetryPolicy);
            }

            return connectorClient;
        }

        /// <summary>
        /// Gets the application credentials. App Credentials are cached so as to ensure we are not refreshing
        /// token everytime.
        /// </summary>
        /// <param name="appId">The application identifier (AAD Id for the bot).</param>
        /// <returns>App credentials.</returns>
        private async Task<MicrosoftAppCredentials> GetAppCredentialsAsync(string appId)
        {
            if (appId == null)
            {
                return MicrosoftAppCredentials.Empty;
            }

            if (!_appCredentialMap.TryGetValue(appId, out var appCredentials))
            {
                string appPassword = await _credentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);
                appCredentials = new MicrosoftAppCredentials(appId, appPassword);
                _appCredentialMap[appId] = appCredentials;
            }

            return appCredentials;
        }
    }
}