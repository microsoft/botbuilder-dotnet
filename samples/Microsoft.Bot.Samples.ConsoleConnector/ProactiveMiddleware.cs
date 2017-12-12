using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples
{
    public class ProactiveMiddleware : IReceiveActivity
    {
        public async Task<ReceiveResponse> ReceiveActivity(BotContext context)
        {
            if (context.IfIntent("delayIntent"))
            {
                int delay = context.TopIntent.Entities[0].ValueAs<int>();

                context.Reply($"Scheduling callback in {delay} milliseconds.");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () =>
                {
                    await Task.Delay(delay).ConfigureAwait(false);
                    await context.Bot.CreateContext(context.ConversationReference, async (context2) =>
                    {
                        context2.Reply($"--> Delayed {delay} milliseconds. <--");
                    }).ConfigureAwait(false);
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                return new ReceiveResponse(true);
            }

            return new ReceiveResponse(false);
        }
    }
}
