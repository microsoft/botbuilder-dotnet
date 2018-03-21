// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters
{
    public class BotFrameworkAdapter : BotAdapter
    {
        private readonly ICredentialProvider _credentialProvider;
        private readonly HttpClient _httpClient;
        private Dictionary<string, MicrosoftAppCredentials> _appCredentialMap = new Dictionary<string, MicrosoftAppCredentials>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkAdapter"/> class.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="middleware">The middleware to use. Use <see cref="MiddlewareSet" class to register multiple middlewares together./></param>
        public BotFrameworkAdapter(ICredentialProvider credentialProvider, HttpClient httpClient = null, IMiddleware middleware = null)
        {
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _httpClient = httpClient ?? new HttpClient();

            if (middleware != null)
            {
                this.Use(middleware);
            }
        }

        public Task ContinueConversation(string botAppId, ConversationReference reference, Func<ITurnContext, Task> callback)
        {
            if (string.IsNullOrWhiteSpace(botAppId))
                throw new ArgumentNullException(nameof(botAppId));

            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback)); 

            var context = new BotFrameworkTurnContext(botAppId, this, reference.GetPostToBotMessage());
            return RunPipeline(context, callback);
        }

        public BotFrameworkAdapter(string appId, string appPassword, HttpClient httpClient = null, IMiddleware middleware = null) 
            : this(new SimpleCredentialProvider(appId, appPassword), httpClient, middleware)
        {
        }

        public new BotFrameworkAdapter Use(IMiddleware middleware)
        {
            base._middlewareSet.Use(middleware);
            return this;
        }

        public async Task ProcessActivity(string authHeader, Activity activity, Func<ITurnContext, Task> callback)
        {
            BotAssert.ActivityNotNull(activity);
            ClaimsIdentity claimsIdentity =  await JwtTokenValidation.AuthenticateRequest(activity, authHeader, _credentialProvider, _httpClient);

            // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For 
            // unauthenticated requests we have anonymouse identity provided auth is disabled.
            string botAppId = GetBotId(claimsIdentity);
            var context = new BotFrameworkTurnContext(botAppId, this, activity);
            await base.RunPipeline(context, callback).ConfigureAwait(false);
        }

        public override async Task<ResourceResponse[]> SendActivities(ITurnContext context, Activity[] activities)
        {
            AssertBotFrameworkContext (context);
            List<ResourceResponse> responses = new List<ResourceResponse>(); 

            BotFrameworkTurnContext bfContext = context as BotFrameworkTurnContext;
            MicrosoftAppCredentials appCredentials = await GetAppCredentials(bfContext.BotAppId);

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
                    var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);
                    response = await connectorClient.Conversations.SendToConversationAsync(activity).ConfigureAwait(false);
                }

                // Collect all the responses that come from the service. 
                responses.Add(response); 
            }

            return responses.ToArray();
        }

        public override async Task<ResourceResponse> UpdateActivity(ITurnContext context, Activity activity)
        {
            AssertBotFrameworkContext(context);

            BotFrameworkTurnContext bfContext = context as BotFrameworkTurnContext;
            MicrosoftAppCredentials appCredentials = await GetAppCredentials(bfContext.BotAppId);
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);
            return await connectorClient.Conversations.UpdateActivityAsync(activity);
        }

        public override async Task DeleteActivity(ITurnContext context, ConversationReference reference)
        {
            AssertBotFrameworkContext(context);

            BotFrameworkTurnContext bfContext = context as BotFrameworkTurnContext;
            MicrosoftAppCredentials appCredentials = await GetAppCredentials(bfContext.BotAppId);
            var connectorClient = new ConnectorClient(new Uri(context.Request.ServiceUrl), appCredentials);
            await connectorClient.Conversations.DeleteActivityAsync(reference.Conversation.Id, reference.ActivityId);
        }

        /// <summary>
        /// Create a conversation on the Bot Framework for the given serviceUrl
        /// </summary>
        /// <param name="serviceUrl">serviceUrl you want to use</param>
        /// <param name="credentials">credentials</param>
        /// <param name="conversationParameters">arguments for the conversation you want to create</param>
        /// <param name="callback">callback which will have the context.Request.Conversation.Id in it</param>
        /// <returns></returns>
        public virtual async Task CreateConversation(string channelId, string serviceUrl, MicrosoftAppCredentials credentials, ConversationParameters conversationParameters, Func<ITurnContext, Task> callback)
        {
            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);
            var result = await connectorClient.Conversations.CreateConversationAsync(conversationParameters);

            // create conversation Update to represent the result of creating the conversation
            var conversationUpdate = Activity.CreateConversationUpdateActivity();
            conversationUpdate.ChannelId = channelId;
            conversationUpdate.TopicName = conversationParameters.TopicName;
            conversationUpdate.ServiceUrl = serviceUrl;
            conversationUpdate.MembersAdded = conversationParameters.Members;
            conversationUpdate.Id = result.ActivityId ?? Guid.NewGuid().ToString("n");
            conversationUpdate.Conversation = new ConversationAccount(id: result.Id);
            conversationUpdate.Recipient = conversationParameters.Bot;

            TurnContext context = new TurnContext(this, (Activity)conversationUpdate);
            await this.RunPipeline(context, callback);
        }

        /// <summary>
        /// Gets the application credentials. App Credentials are cached so as to ensure we are not refreshing
        /// token everytime.
        /// </summary>
        /// <param name="appId">The application identifier (AAD Id for the bot).</param>
        /// <returns>App credentials.</returns>
        protected virtual async Task<MicrosoftAppCredentials> GetAppCredentials(string appId)
        {
            if (appId == null)
            {
                return MicrosoftAppCredentials.Empty;
            }

            if (!_appCredentialMap.TryGetValue(appId, out var appCredentials))
            {
                string appPassword = await _credentialProvider.GetAppPasswordAsync(appId);
                appCredentials = new MicrosoftAppCredentials(appId, appPassword);
                _appCredentialMap[appId] = appCredentials;
            }

            return appCredentials;
        }

        /// <summary>
        /// Gets the bot identifier from claims.
        /// </summary>
        /// <param name="claimsIdentity">The claims identity.</param>
        /// <returns>Bot's AAD AppId, if it could be inferred from claims. Null otherwise.</returns>
        private static string GetBotId(ClaimsIdentity claimsIdentity)
        {
            // For requests coming from channels Audience Claim contains the Bot's AAD AppId
            Claim botAppIdClaim = (claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim) 
                ??
                // For requests coming from Emulator AppId claim contains the Bot's AAD AppId.
                claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AppIdClaim));

            // For anonymous requests (requests with no header) appId is not set in claims.
            if (botAppIdClaim != null)
            {
                return botAppIdClaim.Value;
            }
            else
            {
                return null;
            }
        }

        public static void AssertBotFrameworkContext(ITurnContext context)
        {
            BotAssert.ContextNotNull(context); 

            BotFrameworkTurnContext bfContext = context as BotFrameworkTurnContext;
            if (bfContext == null)
                throw new InvalidOperationException($"BotFramework Context is required. Incorrect context type: {context.GetType().Name}");
        }
    }
}