using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class SetTestOptionsMiddleware : IMiddleware
    {
        public SetTestOptionsMiddleware()
        {
        }

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
