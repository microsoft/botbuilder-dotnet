// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
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
            _credentialProvider = credentialProvider;
            _httpClient = httpClient;

            if (middleware != null)
            {
                this.Use(middleware);
            }
        }

        public BotFrameworkAdapter(string appId, string appPassword, HttpClient httpClient = null, IMiddleware middleware = null) 
            : this(new SimpleCredentialProvider(appId, appPassword), httpClient, middleware)
        {
        }

        public BotFrameworkAdapter Use(Middleware.IMiddleware middleware)
        {
            base._middlewareSet.Use(middleware);
            return this;
        }

        public async Task ProcessActivty(string authHeader, IActivity activity, Func<IBotContext, Task> callback)
        {
            BotAssert.ActivityNotNull(activity);
            ClaimsIdentity claimsIdentity =  await JwtTokenValidation.EnsureValidActivity(activity, authHeader, _credentialProvider, _httpClient);

            // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For 
            // unauthenticated requests we have anonymouse identity provided auth is disabled.
            string botAppId = (claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim) ??
                claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AppIdClaim))?.Value;
            var context = new BotFrameworkBotContext(botAppId, this, activity);
            await base.RunPipeline(context, callback).ConfigureAwait(false);
        }

        protected async override Task SendActivitiesImplementation(IBotContext context, IEnumerable<IActivity> activities)
        {
            foreach (var activity in activities)
            {
                if (activity.Type == ActivityTypesEx.Delay)
                {
                    // The Activity Schema doesn't have a delay type build in, so it's simulated
                    // here in the Bot. This matches the behavior in the Node connector. 
                    int delayMs = (int)((Activity)activity).Value;
                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
                else
                {
                    var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), await GetAppCredentials((context as BotFrameworkBotContext).BotAppId));
                    await connectorClient.Conversations.SendToConversationAsync((Activity)activity).ConfigureAwait(false);
                }
            }
        }

        protected override async Task<ResourceResponse> UpdateActivityImplementation(IBotContext context, IActivity activity)
        {
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), await GetAppCredentials((context as BotFrameworkBotContext).BotAppId));
            return await connectorClient.Conversations.UpdateActivityAsync((Activity)activity);
        }

        protected override async Task DeleteActivityImplementation(IBotContext context, string conversationId, string activityId)
        {
            var connectorClient = new ConnectorClient(new Uri(context.Request.ServiceUrl), await GetAppCredentials((context as BotFrameworkBotContext).BotAppId));
            await connectorClient.Conversations.DeleteActivityAsync(conversationId, activityId);
        }

        protected override Task CreateConversationImplementation()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the application credentials.
        /// </summary>
        /// <param name="appId">The application identifier (AAD Id for the bot).</param>
        /// <returns>App credentials.</returns>
        protected virtual async Task<MicrosoftAppCredentials> GetAppCredentials(string appId)
        {
            MicrosoftAppCredentials appCredentials;
            if (!_appCredentialMap.TryGetValue(appId, out appCredentials))
            {
                string appPassword = await _credentialProvider.GetAppPasswordAsync(appId);
                appCredentials = new MicrosoftAppCredentials(appId, appPassword);
                _appCredentialMap[appId] = appCredentials;
            }

            return appCredentials;
        }
    }
}
