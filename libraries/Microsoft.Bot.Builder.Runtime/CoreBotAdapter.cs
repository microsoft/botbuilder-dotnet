// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Builders.Middleware;
using Microsoft.Bot.Builder.Runtime.Settings;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.Runtime
{
    /// <summary>
    /// Defines the bot runtime standard implementation of <see cref="BotFrameworkHttpAdapter"/>.
    /// </summary>
    internal class CoreBotAdapter : BotFrameworkHttpAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreBotAdapter"/> class.
        /// </summary>
        /// <param name="services">Services registered with the application.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="options">Configured options for the <see cref="CoreBotAdapter"/> instance.</param>
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

            this.OnTurnError = options.Value.OnTurnError.Build(services, configuration);
        }
    }
}
