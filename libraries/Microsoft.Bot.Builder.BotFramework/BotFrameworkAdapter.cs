// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.BotFramework
{
    public class BotFrameworkAdapter : BotAdapter
    {
        private readonly SimpleCredentialProvider _credentialProvider;
        private readonly MicrosoftAppCredentials _credentials;
        private readonly HttpClient _httpClient; 

        public BotFrameworkAdapter(IConfiguration configuration, HttpClient httpClient=null) : base()
        {
            _httpClient = httpClient ?? new HttpClient();
            _credentialProvider = new ConfigurationCredentialProvider(configuration);
            _credentials = new MicrosoftAppCredentials(_credentialProvider.AppId, _credentialProvider.Password);                                   
        }

        public BotFrameworkAdapter(string appId, string appPassword, HttpClient httpClient = null) : base()
        {
            _httpClient = httpClient ?? new HttpClient();
            _credentials = new MicrosoftAppCredentials(appId, appPassword);
            _credentialProvider = new SimpleCredentialProvider(appId, appPassword);
        }

        public new BotFrameworkAdapter Use(Middleware.IMiddleware middleware)
        {
            base._middlewareSet.Use(middleware);
            return this;
        }

        public async Task ProcessActivty(string authHeader, Activity activity, Func<IBotContext,Task> callback)
        {
            BotAssert.ActivityNotNull(activity);
            await JwtTokenValidation.AssertValidActivity(activity, authHeader, _credentialProvider, _httpClient);

            await base.ProcessActivityInternal(activity, callback).ConfigureAwait(false);
        }

        protected async override Task SendActivityImplementation(IBotContext context, IActivity activity)
        {
            if (activity.Type == "delay")
            {
                // The Activity Schema doesn't have a delay type build in, so it's simulated
                // here in the Bot. This matches the behavior in the Node connector. 
                int delayMs = (int)((Activity)activity).Value;
                await Task.Delay(delayMs).ConfigureAwait(false);
            }
            else
            {
                var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), _credentials);
                await connectorClient.Conversations.SendToConversationAsync((Activity)activity).ConfigureAwait(false);
            }
        }

        protected override Task<ResourceResponse> UpdateActivityImplementation(IBotContext context, IActivity activity)
        {
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), _credentials);
            return connectorClient.Conversations.UpdateActivityAsync((Activity)activity);
        }

        protected override Task DeleteActivityImplementation(IBotContext context, string conversationId, string activityId)
        {
            var connectorClient = new ConnectorClient(new Uri(context.Request.ServiceUrl), _credentials);
            return connectorClient.Conversations.DeleteActivityAsync(conversationId, activityId);
        }

        protected override Task CreateConversationImplementation()
        {
            throw new NotImplementedException();
        }
    }
}
