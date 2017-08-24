using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.Bot.Samples.Connector.EchoBot
{
    public class EchoMiddleWare : IReceiveActivity
    {
        public async Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            var activity = context.Request as Activity;
            var reply = activity.CreateReply();
            if (activity.Type == ActivityTypes.Message)
            {
                reply.Text = $"echo: {activity.Text}";
            }
            else
            {
                reply.Text = $"activity type: {activity.Type}";
            }
            context.Responses.Add(reply);
            await context.Post(token);
            return new ReceiveResponse(true);
        }
    }
}
