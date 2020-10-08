using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Mocks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    /// <summary>
    /// Middleware to add <see cref="MockBotFrameworkClient"/> to the  <see cref="ITurnContext.TurnState"/>.
    /// </summary>
    public class SetBotFrameworkClientMiddleware : IMiddleware
    {
        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                turnContext.TurnState.Add<BotFrameworkClient>(new MockBotFrameworkClient());
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
