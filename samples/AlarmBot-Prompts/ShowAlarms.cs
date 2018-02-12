using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace AlarmBot_Prompts
{
    public class ShowAlarms
    {
        internal Task Begin(IBotContext context)
        {
            // Delete any existing topic
            context.State.Conversation.Remove("topic");

            // Render alarms to user.
            // - No reply is expected so we don't set a new topic.
            RenderAlarms(context);
            return Task.CompletedTask;
        }

        internal static int RenderAlarms(IBotContext context)
        {
            var list = context.State.User["alarms"] as IList<Alarm> ?? new List<Alarm>();
            if (list.Count > 0)
            {
                var msg = "**Current Alarms**\n\n";
                msg += string.Join('\n', list.Select(a => $@"- {a.Title} ({a.Time})"));
                context.Reply(msg);
            }
            else
            {
                context.Reply("No alarms found.");
            }

            return list.Count;
        }
    }
}
