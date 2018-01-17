using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Connector;

namespace Micosoft.Bot.Samples.InjectionBasedBotExample
{
    public class EchoMiddleware : Microsoft.Bot.Builder.Middleware.IReceiveActivity
    {
        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                long turnNumber = context.State.Conversation["turnNumber"] ?? 0;
                context.State.Conversation["turnNumber"] = ++turnNumber;

                context.Responses.Add(
                        ((Activity)context.Request).CreateReply(
                            $"[{turnNumber}] echo: {context.Request.AsMessageActivity().Text}"));
            }

            await next();
        }
    }
}
