// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.FormFlowTest
{
    public class ConsoleAdapter : BotAdapter
    {
        public ConsoleAdapter() : base()
        {
        }

        public new ConsoleAdapter Use(IMiddleware middleware)
        {
            base.Use(middleware);
            return this;
        }

        public async Task ProcessActivity(Func<ITurnContext, Task> callback = null)
        {
            while (true)
            {
                var msg = Console.ReadLine();
                if (msg == null)
                    break;

                var activity = new MessageActivity
                {
                    Text = msg,
                    ChannelId = "console",
                    From = new ChannelAccount(id: "user", name: "User1"),
                    Recipient = new ChannelAccount(id: "bot", name: "Bot"),
                    Conversation = new ConversationAccount(id: "Convo1"),
                    Timestamp = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString()
                };

                using (var context = new TurnContext(this, activity))
                {
                    await base.RunPipelineAsync(context, callback, default(CancellationToken));
                }
            }
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext context, Activity[] activities, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (activities == null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            if (activities.Length == 0)
            {
                throw new ArgumentException("Expecting one or more activities, but the array was empty.", nameof(activities));
            }

            var responses = new ResourceResponse[activities.Length];

            for(var index = 0; index < activities.Length; index++)
            {
                var activity = activities[index];

                switch (activity)
                {
                    case MessageActivity messageActivity:
                        {                            

                            if (messageActivity.Attachments != null && messageActivity.Attachments.Any())
                            {
                                var attachment = messageActivity.Attachments.Count == 1 ? "1 attachment" : $"{messageActivity.Attachments.Count} attachments";
                                Console.WriteLine($"{messageActivity.Text} with {attachment} ");
                            }
                            else
                            {
                                Console.WriteLine($"{messageActivity.Text}");
                            }
                        }
                        break;
                    case DelayActivity delayActivity:
                        {
                            // The Activity Schema doesn't have a delay type build in, so it's simulated
                            // here in the Bot. This matches the behavior in the Node connector. 
                            await Task.Delay(delayActivity.Delay).ConfigureAwait(false);
                        }
                        break;
                    case TraceActivity traceActivity:
                        // don't send trace activities unless you know that the client needs them.  For example: BF protocol only sends Trace Activity when talking to emulator channel
                        break;

                    default:
                        Console.WriteLine("Bot: activity type: {0}", activity.Type);
                        break;
                }

                responses[index] = new ResourceResponse(activity.Id);
            }

            return responses;
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext context, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteActivityAsync(ITurnContext context, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }
}
