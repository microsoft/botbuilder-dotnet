// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.Adapters.Webex.TestBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendWelcomeMessageAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Attachments != null)
            {
                var activity = MessageFactory.Text($" I got {turnContext.Activity.Attachments.Count} attachments");
                foreach (var attachment in turnContext.Activity.Attachments)
                {
                    var image = new Schema.Attachment(
                        "image/png",
                        "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU");

                    activity.Attachments.Add(image);
                }

                await turnContext.SendActivityAsync(activity, cancellationToken);
            }
            else
            {
                var activity = MessageFactory.Text($"Echo: {turnContext.Activity.Text}");
                await turnContext.SendActivityAsync(activity, cancellationToken);
            }
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            Activity activity;

            if (turnContext.Activity.Value != null)
            {
                var inputs = (Dictionary<string, string>)turnContext.Activity.Value;
                var name = inputs["Name"];

                activity = MessageFactory.Text($"How are you doing {name}?");
                await turnContext.SendActivityAsync(activity, cancellationToken);
            }

            if (turnContext.Activity.TryGetChannelData(out WebhookEventData eventData))
            {
                if (eventData.ResourceName == "memberships" && eventData.EventTypeName == "created")
                {
                    activity = MessageFactory.Text("Hello! I'm the WebexTestBot, nice to meet you");
                    activity.Conversation = new ConversationAccount { Id = eventData.SpaceData.Id };

                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var activity = MessageFactory.Attachment(CreateAdaptiveCardAttachment(Directory.GetCurrentDirectory() + @"\Resources\adaptive_card.json"));

                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }
        }

        private static Schema.Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Schema.Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson, new JsonSerializerSettings { MaxDepth = null }),
            };
            return adaptiveCardAttachment;
        }
    }
}
