using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace CoreFunctionBot
{
    internal class FunctionBotAdapter : CloudAdapter
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionBotAdapter"/> class.
        /// </summary>
        /// <param name="botFrameworkAuthentication">An instance of a BotFrameworkAuthentication.</param>
        /// <param name="middlewares">The collection of middleware instances.</param>
        /// <param name="logger">The logger instance.</param>
        public FunctionBotAdapter(
            BotFrameworkAuthentication botFrameworkAuthentication,
            ILogger<FunctionBotAdapter> logger = null)
            : base(botFrameworkAuthentication, logger)
        {
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
