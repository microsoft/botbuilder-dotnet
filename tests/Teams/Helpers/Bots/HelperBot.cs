// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class HelperBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            if (turnContext.Activity.Text == "notify")
            {
                var msg = MessageFactory.Text("This message will contain a notification");
                msg.NotifyUser();
                msg.ApplyConversationReference(turnContext.Activity.GetConversationReference());

                await turnContext.SendActivityAsync(msg, cancellationToken);
            }
            else if (turnContext.Activity.Text == "get general channel")
            {
                var msg = MessageFactory.Text($"The ID of the general channel is {turnContext.Activity.GetGeneralChannel().Id}");
                msg.ApplyConversationReference(turnContext.Activity.GetConversationReference());
                await turnContext.SendActivityAsync(msg, cancellationToken);
            }
            else if (turnContext.Activity.Text == "send msg to general channel")
            {
                var msg = MessageFactory.Text("This message will appear in the teams general channel");
                msg.ApplyConversationReference(turnContext.Activity.GetConversationReference());
                turnContext.Activity.AddressMessageToTeamsGeneralChannel();
                msg.ChannelData = turnContext.Activity.ChannelData;
                await turnContext.SendActivityAsync(msg, cancellationToken);
            }
            else
            {
                var msg = MessageFactory.Text("Send me \"notify\" to get a notificaiton. Send me \"get general channel\" to get the ID of the general channel." +
                    "Send me \"send msg to general channel\" to send a message to the general channel");
                await turnContext.SendActivityAsync(msg, cancellationToken);
            }
        }
    }
}
