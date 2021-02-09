// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Integration.Settings;
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
        public CoreBotAdapter(IServiceProvider services, IConfiguration configuration)
            : base(
                services.GetService<ICredentialProvider>(),
                services.GetService<AuthenticationConfiguration>(),
                services.GetService<IChannelProvider>(),
                logger: services.GetService<ILogger<BotFrameworkHttpAdapter>>())
        {
            var conversationState = services.GetService<ConversationState>();
            var userState = services.GetService<UserState>();

            this.UseStorage(services.GetService<IStorage>())
                .UseBotState(userState, conversationState)
                .Use(new RegisterClassMiddleware<IConfiguration>(configuration));

            // Pick up feature based middlewares: telemetry, inspection, transcripts, etc
            var middlewares = services.GetServices<IMiddleware>();

            foreach (IMiddleware middleware in middlewares)
            {
                this.Use(middleware);
            }

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application
                Logger.LogError(exception, exception.Message);

                // Send the exception message to the user. Since the default behavior does not
                // send logs or trace activities, the bot appears hanging without any activity
                // to the user.
                await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        await conversationState.DeleteAsync(turnContext).ConfigureAwait(false);
                    }
#pragma warning disable CA1031 // Do not catch general exception types (we just log the exception and continue)
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        Logger.LogError(ex, $"Exception caught on attempting to Delete ConversationState : {ex.Message}");
                    }
                }
            };
        }
    }
}
