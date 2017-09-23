using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples.Middleware
{
    public class EchoMiddleWare : IReceiveActivity
    {
        public async Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            if (context.IfIntent("echoIntent"))
            {
                context.Responses.Add(
                        context.Request.CreateReply(
                            $"echo: {context.Request.Text.Substring("echo ".Length)}"));                
                
                return new ReceiveResponse(true);
            }
            return new ReceiveResponse(false);
        }
    }
}