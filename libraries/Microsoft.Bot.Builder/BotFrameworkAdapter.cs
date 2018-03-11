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
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _httpClient = httpClient ?? new HttpClient();

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

        public async Task ProcessActivity(string authHeader, Activity activity, Func<IBotContext, Task> callback)
        {
            BotAssert.ActivityNotNull(activity);
            ClaimsIdentity claimsIdentity =  await JwtTokenValidation.AuthenticateRequest(activity, authHeader, _credentialProvider, _httpClient);

            // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For 
            // unauthenticated requests we have anonymouse identity provided auth is disabled.
            string botAppId = GetBotId(claimsIdentity);
            var context = new BotFrameworkBotContext(botAppId, this, activity);
            await base.RunPipeline(context, callback).ConfigureAwait(false);
        }

        public async override Task SendActivities(IBotContext context, IEnumerable<Activity> activities)
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
                    MicrosoftAppCredentials appCredentials = await GetAppCredentials((context as BotFrameworkBotContext).BotAppId);
                    var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);
                    await connectorClient.Conversations.SendToConversationAsync((Activity)activity).ConfigureAwait(false);
                }
            }
        }

        public override async Task<ResourceResponse> UpdateActivity(IBotContext context, Activity activity)
        {
            MicrosoftAppCredentials appCredentials = await GetAppCredentials((context as BotFrameworkBotContext).BotAppId);
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);
            return await connectorClient.Conversations.UpdateActivityAsync((Activity)activity);
        }

        public override async Task DeleteActivity(IBotContext context, string conversationId, string activityId)
        {
            MicrosoftAppCredentials appCredentials = await GetAppCredentials((context as BotFrameworkBotContext).BotAppId);
            var connectorClient = new ConnectorClient(new Uri(context.Request.ServiceUrl), appCredentials);
            await connectorClient.Conversations.DeleteActivityAsync(conversationId, activityId);
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
    }
}
