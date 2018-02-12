using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace AlarmBot_Prompts
{
    public class DeleteAlarm
    {
        internal Task Begin(IBotContext context)
        {
            // Delete any existing topic
            context.State.Conversation.Remove("topic");

            // Render list of topics to user
            var count = ShowAlarms.RenderAlarms(context);
            if (count > 0)
            {
                // Set topic and prompt user for alarm to delete.
                context.State.Conversation["topic"] = "deleteAlarm";
                context.Reply(@"Which alarm would you like to delete?");
            }

            return Task.CompletedTask;
        }

        internal Task RouteReply(IBotContext context)
        {
            // Validate users reply and delete alarm
            var deleted = false;
            var title = context.Request.AsMessageActivity()?.Text.Trim();
            var list = (context.State.User["alarms"] as IList<Alarm>) ?? new List<Alarm>();

            var alarmsToRemove = list.Where(a => a.Title.ToLowerInvariant() == title.ToLowerInvariant()).ToList();
            deleted = alarmsToRemove.Any();
            foreach (var a in alarmsToRemove) list.Remove(a);


            // Notify user of deletion or re-prompt
            if (deleted)
            {
                context.Reply($@"Deleted the ""{title}"" alarm.");
                context.State.Conversation.Remove("topic");
            }
            else
            {
                context.Reply($@"An alarm named ""{title}"" doesn't exist. Which alarm would you like to delete? Say ""cancel"" to quit.");
            }

            return Task.CompletedTask;
        }
    }
}
