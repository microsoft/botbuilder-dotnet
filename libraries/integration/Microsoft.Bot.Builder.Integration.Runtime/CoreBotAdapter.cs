// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.Runtime
{
    /// <summary>
    /// Defines the bot runtime standard implementation of <see cref="BotFrameworkHttpAdapter"/>.
    /// </summary>
    internal class CoreBotAdapter : BotFrameworkHttpAdapter
    {
        private ConversationState _conversationState;

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
                Use(middleware);
            }

            _conversationState = conversationState;

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                Logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                await SendErrorMessageAsync(turnContext, exception).ConfigureAwait(false);
                await SendEoCToParentIfSkillAsync(turnContext, exception).ConfigureAwait(false);
                await ClearConversationStateAsync(turnContext).ConfigureAwait(false);
            };
        }

        private async Task SendErrorMessageAsync(ITurnContext turnContext, Exception exception)
        {
            try
            {
                // Send a message to the user.
                var errorMessageText = "The bot encountered an error or bug.";
                var errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.IgnoringInput);
                await turnContext.SendActivityAsync(errorMessage).ConfigureAwait(false);

                errorMessageText = "To continue to run this bot, please fix the bot source code.";
                errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(errorMessage).ConfigureAwait(false);

                // Send a trace activity, which will be displayed in the Bot Framework Emulator.
                // Note: we return the entire exception in the value property to help the developer;
                // this should not be done in production.
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.ToString(), "https://www.botframework.com/schemas/error", "TurnError").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Exception caught in SendErrorMessageAsync : {ex}");
            }
        }

        private async Task SendEoCToParentIfSkillAsync(ITurnContext turnContext, Exception exception)
        {
            if (IsSkillBot(turnContext))
            {
                try
                {
                    // Send an EndOfConversation activity to the skill caller with the error to end the conversation,
                    // and let the caller decide what to do.
                    var endOfConversation = Activity.CreateEndOfConversationActivity();
                    endOfConversation.Code = "SkillError";
                    endOfConversation.Text = exception.Message;
                    await turnContext.SendActivityAsync(endOfConversation).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Exception caught in SendEoCToParentAsync : {ex}");
                }
            }
        }

        private async Task ClearConversationStateAsync(ITurnContext turnContext)
        {
            try
            {
                // Delete the conversationState for the current conversation to prevent the
                // bot from getting stuck in a error-loop caused by being in a bad state.
                // ConversationState should be thought of as similar to "cookie-state" for a Web page.
                await _conversationState.DeleteAsync(turnContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Exception caught on attempting to Delete ConversationState : {ex}");
            }
        }

        private bool IsSkillBot(ITurnContext turnContext)
        {
            return turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity claimIdentity
                && SkillValidation.IsSkillClaim(claimIdentity.Claims);
        }
    }
}
