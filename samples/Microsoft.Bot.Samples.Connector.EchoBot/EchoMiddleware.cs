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
            var activity = context.Request as Activity;
            var reply = activity.CreateReply();

            reply.Text = (activity.Type == ActivityTypes.Message) 
                ? $"echo: {activity.Text}" : $"activity type: {activity.Type}";

            context.Responses.Add(reply);            
            return new ReceiveResponse(true);
        }
    }
}
