using Microsoft.Bot.Builder;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples.ConsoleConnector
{
    public class ReverseMiddleWare : IReceiveActivity
    {
        public async Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            if (context.IfIntent("reverseIntent"))
            {
                string reversedString = new string(
                    context.Request.Text.Substring("reverse ".Length).ToCharArray().Reverse().ToArray());

                context.Responses.Add(
                       context.Request.CreateReply(
                           $"reverse: {reversedString}"));
                
                return new ReceiveResponse(true);
            }

            return new ReceiveResponse(false); 
        }
    }
}
