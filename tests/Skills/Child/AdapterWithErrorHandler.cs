// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        private readonly BotState _conversationState;
        private readonly BotState _userState;

        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger, ConversationState conversationState, UserState userState)
            : base(configuration, logger)
        {
            _conversationState = conversationState;
            _userState = userState;

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send a message to the user
                await turnContext.SendActivityAsync("The bot encounted an error or bug.");
                await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, $"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                //await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            // Save any state changes that might have occured during the turn before sending anything
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);

            return await base.SendActivitiesAsync(turnContext, activities, cancellationToken).ConfigureAwait(false);
        }
    }
}
