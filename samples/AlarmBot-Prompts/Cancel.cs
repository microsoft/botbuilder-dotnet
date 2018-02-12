using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace AlarmBot_Prompts
{
    public class Cancel
    {
        internal Task Begin(IBotContext context)
        {
            // Cancel the current topic
            if (!string.IsNullOrWhiteSpace(context.State.Conversation["topic"]))
            {
                context.State.Conversation.Remove("topic");
                context.Reply(@"Ok...Canceled.");
            }
            else
            {
                context.Reply(@"Nothing to cancel.");
            }

            return Task.CompletedTask;
        }
    }
}
