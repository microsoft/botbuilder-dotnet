using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.Bot.Samples.Connector.EchoBot
{
    public class EchoMiddleWare : IMiddleware
    {
        public Task ContextCreated(BotContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task ContextDone(BotContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task PostAsync(BotContext context, IList<IActivity> acitivties, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task<bool> ReceiveActivity(BotContext context, CancellationToken token)
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
            await context.PostAsync(token);
            return true;
        }
    }
}
