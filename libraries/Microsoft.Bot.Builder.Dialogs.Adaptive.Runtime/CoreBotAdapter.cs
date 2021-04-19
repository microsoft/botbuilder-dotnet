// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime
{
    internal class CoreBotAdapter : CloudAdapter
    {
        public CoreBotAdapter(
            BotFrameworkAuthentication botFrameworkAuthentication,
            IEnumerable<IMiddleware> middlewares,
            ILogger logger = null)
            : base(botFrameworkAuthentication, logger)
        {
            // Pick up feature based middlewares such as telemetry or transcripts
            foreach (IMiddleware middleware in middlewares)
            {
                Use(middleware);
            }

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                Logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send the exception message to the user. Since the default behavior does not
                // send logs or trace activities, the bot appears hanging without any activity
                // to the user.
                await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);

                var conversationState = turnContext.TurnState.Get<ConversationState>();

                if (conversationState != null)
                {
                    // Delete the conversationState for the current conversation to prevent the
                    // bot from getting stuck in a error-loop caused by being in a bad state.
                    await conversationState.DeleteAsync(turnContext).ConfigureAwait(false);
                }
            };
        }
    }
}
