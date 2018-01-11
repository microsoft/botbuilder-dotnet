using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Samples.Middleware
{
    public class EchoMiddleware : Microsoft.Bot.Builder.Middleware.IReceiveActivity
    {        
        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            // Only deal with messages. Ignore all else. 
            if (context.Request.Type == ActivityTypes.Message)
            {
                if (context.IfIntent("echoIntent"))
                {
                    long turnNumber = context.State.Conversation["turnNumber"] ?? 0;
                    context.State.Conversation["turnNumber"] = ++turnNumber;

                    context.Reply($"[{turnNumber}] echo: {context.TopIntent.Entities[0].ValueAs<string>()}");
                }
            }
            await next();
        }
    }
}