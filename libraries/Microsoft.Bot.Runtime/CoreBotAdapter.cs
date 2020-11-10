// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Runtime.Builders.Middleware;
using Microsoft.Bot.Runtime.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Runtime
{
    public class CoreBotAdapter : BotFrameworkHttpAdapter
    {
        public CoreBotAdapter(
            IServiceProvider services,
            IConfiguration configuration,
            IOptions<CoreBotAdapterOptions> options)
            : base(
                services.GetService<ICredentialProvider>(),
                services.GetService<AuthenticationConfiguration>(),
                services.GetService<IChannelProvider>(),
                logger: services.GetService<ILogger<BotFrameworkHttpAdapter>>())
        {
            var conversationState = services.GetService<ConversationState>();
            var userState = services.GetService<UserState>();

            this.UseStorage(services.GetService<IStorage>());
            this.UseBotState(userState, conversationState);
            this.Use(new RegisterClassMiddleware<IConfiguration>(configuration));

            foreach (IMiddlewareBuilder middleware in options.Value.Middleware)
            {
                this.Use(middleware.Build(services, configuration));
            }

            this.OnTurnError = async (turnContext, exception) =>
            {
                await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);
                await conversationState.ClearStateAsync(turnContext).ConfigureAwait(false);
                await conversationState.SaveChangesAsync(turnContext).ConfigureAwait(false);
            };
        }
    }
}
