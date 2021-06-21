using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    /// <summary>
    /// Middleware that catch "SetTestOptions" event and save into "Conversation.TestOptions".
    /// </summary>
    public class SetTestOptionsMiddleware : IMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetTestOptionsMiddleware"/> class.
        /// </summary>
        public SetTestOptionsMiddleware()
        {
        }

        /// <summary>
        /// Processes an incoming event activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Event)
            {
                var eventActivity = turnContext.Activity.AsEventActivity();
                if (eventActivity.Name == "SetTestOptions")
                {
                    var conversationState = turnContext.TurnState.Get<ConversationState>();
                    var property = conversationState.CreateProperty<object>("TestOptions");
                    await property.SetAsync(turnContext, eventActivity.Value).ConfigureAwait(false);
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
