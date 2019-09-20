// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples.Bots
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;

    public class OneOnOneAlertBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text == "card")
            {
                var heroCard = new HeroCard(text: "You will receive a notification in your Activity feed.");
                var msg = MessageFactory.Attachment(heroCard.ToAttachment());
                msg.Summary = "This text will only show if the activty has a card attachment";
                msg.ChannelData = new TeamsChannelData
                {
                    Notification = new NotificationInfo
                    {
                        Alert = true,
                    },
                };

                await turnContext.SendActivityAsync(msg, cancellationToken);
            }
            else if (turnContext.Activity.Text == "message")
            {
                var msg = MessageFactory.Text("you will receive a notification in your Activity feed.");
                msg.Summary = "You won't see this text";
                msg.ChannelData = new TeamsChannelData
                {
                    Notification = new NotificationInfo
                    {
                        Alert = true,
                    },
                };

                await turnContext.SendActivityAsync(msg, cancellationToken);
            }
            else
            {
                var msg = MessageFactory.Text("Send me \"card\" to receive a card. Send me \"message\" to receive a message");
                await turnContext.SendActivityAsync(msg, cancellationToken);
            }
        }
    }
}
