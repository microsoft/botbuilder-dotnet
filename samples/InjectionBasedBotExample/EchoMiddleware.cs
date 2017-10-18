using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Micosoft.Bot.Samples.InjectionBasedBotExample
{
    public class EchoMiddleware : IReceiveActivity
    {
        public async Task<ReceiveResponse> ReceiveActivity(BotContext context)
        {
            long turnNumber = context.State.Conversation["turnNumber"] ?? 0;
            context.State.Conversation["turnNumber"] = ++turnNumber;

            context.Responses.Add(
                    context.Request.CreateReply($"[{turnNumber}] echo: {context.Request.Text}"));

            return new ReceiveResponse(true);
        }
    }
}
