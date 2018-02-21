using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace AlarmBot_Prompts
{
    public class AddAlarm
    {
        internal Task Begin(IBotContext context)
        {
            // Set topic and initialize empty alarm
            context.State.Conversation["topic"] = "addAlarm";
            context.State.Conversation["alarm"] = new Alarm();
            return NextField(context);
        }

        private Task NextField(IBotContext context)
        {
            // Prompt user for next missing field
            if (string.IsNullOrWhiteSpace(context.State.Conversation["alarm.title"]))
            {
                context.Reply("What would you like to call your alarm?");
                context.State.Conversation["prompt"] = "title";
            }
            else if (string.IsNullOrWhiteSpace(context.State.Conversation["alarm.time"]))
            {
                context.Reply($@"What time would you like to set the ""{context.State.Conversation["alarm.title"]}"" alarm for?");
                context.State.Conversation["prompt"] = "time";
            }
            else
            {
                // Alarm completed so set alarm.
                var list = (context.State.User["alarms"] as IList<Alarm>) ?? new List<Alarm>();
                var newAlarm = new Alarm
                {
                    Title = context.State.Conversation["alarm.title"],
                    Time = context.State.Conversation["alarm.time"]
                };
                list.Add(newAlarm);
                context.State.User["alarms"] = list;

                // TODO: set alarm
                // Notify user and cleanup topic state
                context.Reply($@"Your alarm named ""{newAlarm.Title}"" is set for {newAlarm.Time}.");
                context.State.Conversation.Remove("topic");
                context.State.Conversation.Remove("alarm.title");
                context.State.Conversation.Remove("alarm.time");
                context.State.Conversation.Remove("prompt");
            }

            return Task.CompletedTask;
        }

        internal Task RouteReply(IBotContext context)
        {
            // Handle users reply to prompt
            var utterance = context.Request.AsMessageActivity()?.Text.Trim();
            switch (context.State.Conversation["prompt"])
            {
                case "title":
                    // Validate reply and save to alarm
                    if (utterance.Length > 2)
                    {
                        context.State.Conversation["alarm.title"] = utterance;
                    }
                    else
                    {
                        context.Reply("I'm sorry. Your alarm should have a title at least 3 characters long.");
                    }
                    break;
                case "time":
                    // TODO: validate time user replied with
                    context.State.Conversation["alarm.time"] = utterance;
                    break;
            }

            return NextField(context);
        }
    }
}
