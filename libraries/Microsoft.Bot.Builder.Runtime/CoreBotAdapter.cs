// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Builders.Middleware;
using Microsoft.Bot.Builder.Runtime.Settings;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest.TransientFaultHandling;

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
        /// <param name="options">Configured options for the <see cref="CoreBotAdapter"/> instance.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public CoreBotAdapter(
            IServiceProvider services,
            IOptions<CoreBotAdapterOptions> options,
            IConfiguration configuration,
            ICredentialProvider credentialProvider = null,
            AuthenticationConfiguration authConfig = null,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            ILogger logger = null)
            : base(configuration, credentialProvider: credentialProvider, authConfig: authConfig, channelProvider: channelProvider, connectorClientRetryPolicy: connectorClientRetryPolicy, customHttpClient: customHttpClient, logger: logger)
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
