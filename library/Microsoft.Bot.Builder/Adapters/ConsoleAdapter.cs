using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Adapters
{
    public class ConsoleAdapter : ActivityAdapterBase
    {
        public ConsoleAdapter() : base()
        {
        }

        public async override Task Post(IList<Activity> activities)
        {
            foreach (Activity activity in activities)
            {
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        if (activity.Attachments != null && activity.Attachments.Any())
                        {
                            var attachment = activity.Attachments.Count == 1 ? "1 attachments" : $"{activity.Attachments.Count()} attachments";
                            Console.WriteLine($"{activity.Text} with {attachment} ");
                        }
                        else
                        {
                            Console.WriteLine($"{activity.Text}");
                        }
                        break;
                    case "delay":
                        // The Activity Schema doesn't have a delay type build in, so it's simulated
                        // here in the Bot. This matches the behavior in the Node connector. 
                        int delayMs = (int)activity.Value;
                        await Task.Delay(delayMs);
                        break;
                    default:
                        Console.WriteLine("Bot: activity type: {0}", activity.Type);
                        break;
                }
            }
        }

        public async Task Listen()
        {            
            while (true)
            {
                var msg = Console.ReadLine();
                if (msg == null || msg.ToLower() == "quit")
                {
                    break;
                }

                var activity = new Activity()
                {
                    Text = msg,
                    ChannelId = "console",
                    From = new ChannelAccount(id: "user", name: "User1"),
                    Recipient = new ChannelAccount(id: "bot", name: "Bot"),
                    Conversation = new ConversationAccount(id: "Convo1"),
                    Timestamp = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString(),
                    Type = ActivityTypes.Message
                };

                if (this.OnReceive != null)
                    await this.OnReceive(activity);
            }
        }
    }    
}
