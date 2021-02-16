// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.Runtime
{
    /// <summary>
    /// Defines the bot runtime standard implementation of <see cref="BotFrameworkHttpAdapter"/>.
    /// </summary>
    internal class CoreBotAdapter : BotFrameworkHttpAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreBotAdapter"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="credentialProvider">Credential provider.</param>
        /// <param name="authenticationConfiguration">Authentication configuration for the adapter.</param>
        /// <param name="channelProvider">Channel provider for the adapter.</param>
        /// <param name="storage">Registered storage for the adapter.</param>
        /// <param name="conversationState">Conversation state for the adapter.</param>
        /// <param name="userState">User state for the adapter.</param>
        /// <param name="middlewares">Collection of registered middlewares to be used in the adapter.</param>
        /// <param name="logger">Telemetry logger.</param>
        public CoreBotAdapter(
            IConfiguration configuration,
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authenticationConfiguration,
            IChannelProvider channelProvider,
            IStorage storage,
            ConversationState conversationState,
            UserState userState,
            IEnumerable<IMiddleware> middlewares,
            ILogger<BotFrameworkHttpAdapter> logger)
            : base(credentialProvider, authenticationConfiguration, channelProvider, logger: logger)
        {
            this.UseStorage(storage)
                .UseBotState(userState, conversationState)
                .Use(new RegisterClassMiddleware<IConfiguration>(configuration));

            // Pick up feature based middlewares such as telemetry or transcripts
            foreach (IMiddleware middleware in middlewares)
            {
                this.Use(middleware);
            }

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application
                Logger.LogError(exception, exception.Message);

                try
                {
                    // Send the exception message to the user. Since the default behavior does not
                    // send logs or trace activities, the bot appears hanging without any activity
                    // to the user.
                    await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);

                    if (conversationState != null)
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        await conversationState.DeleteAsync(turnContext).ConfigureAwait(false);
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types (we just log the exception and continue)
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    Logger.LogError(ex, $"Exception caught on attempting to Delete ConversationState : {ex.Message}");
                }
            };
        }
    }
}
