using Microsoft.Bot.Builder;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Samples
{
    public class ReverseMiddleWare : Microsoft.Bot.Builder.Middleware.IReceiveActivity
    {
        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            // Only deal with messages. Ignore all else. 
            if (context.Request.Type == ActivityTypes.Message)
            {
                if (context.IfIntent("reverseIntent"))
                {
                    string reversedString = new string(
                        context.Request.AsMessageActivity().Text.Substring("reverse ".Length).ToCharArray().Reverse().ToArray());

                    context.Reply($"reverse: {reversedString}");
                }
            }
            await next(); 
        }
    }
}