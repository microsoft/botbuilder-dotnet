// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters
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

        public override async Task ProcessActivity(Activity activity, Func<ITurnContext, Task> callback = null, CancellationToken cancelToken = default(CancellationToken))
        {
            var context = new TurnContext(this, activity);
            await base.RunPipeline(context, callback);
        }

        public override async Task<ResourceResponse[]> SendActivities(ITurnContext context, Activity[] activities)
        {
            List<ResourceResponse> responses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                responses.Add(new ResourceResponse(activity.Id));

                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                        {                            

                            IMessageActivity message = activity.AsMessageActivity();
                            if (message.Attachments != null && message.Attachments.Any())
                            {
                                var attachment = message.Attachments.Count == 1 ? "1 attachments" : $"{message.Attachments.Count()} attachments";
                                Console.WriteLine($"{message.Text} with {attachment} ");
                            }
                            else
                            {
                                Console.WriteLine($"{message.Text}");
                            }
                        }
                        break;
                    case ActivityTypesEx.Delay:
                        {
                            // The Activity Schema doesn't have a delay type build in, so it's simulated
                            // here in the Bot. This matches the behavior in the Node connector. 
                            int delayMs = (int)((Activity)activity).Value;
                            await Task.Delay(delayMs).ConfigureAwait(false);
                        }
                        break;
                    default:
                        Console.WriteLine("Bot: activity type: {0}", activity.Type);
                        break;
                }
            }

            return responses.ToArray();
        }

        public override Task<ResourceResponse> UpdateActivity(ITurnContext context, Activity activity)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteActivity(ITurnContext context, ConversationReference reference)
        {
            throw new NotImplementedException();
        }

    }
}
