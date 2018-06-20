// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace AspNetCore_CardActions_Bot
{
    public class CardActionsBot : IBot
    {
        public async Task OnTurn(ITurnContext context)
        {
            switch (context.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    var newUserName = context.Activity.MembersAdded.FirstOrDefault()?.Name;
                    if (!string.IsNullOrWhiteSpace(newUserName) && newUserName != "Bot")
                    {
                        await context.SendActivity($"Welcome to the CardActions-bot!");
                        await context.SendActivity(GetSuggestedActions());
                    }

                    break;
                case ActivityTypes.Message:
                    if (IsBackAction(context))
                    {
                        await context.SendActivity($"You sent \"_{context.Activity.Text}_\"");
                    }
                    await context.SendActivity(GetSuggestedActions());
                    break;
            }
        }

        private IActivity GetSuggestedActions()
        {
            var suggestedActions = new CardAction[]
            {
                new CardAction
                {
                    Type = ActionTypes.ImBack,
                    Title = "ImBack",
                    Value = "message to bot from ImBack action"
                },
                new CardAction
                {
                    Type = ActionTypes.PostBack,
                    Title = "PostBack",
                    Value = "hidden message from PostBack action"
                },
                new CardAction
                {
                    Type = ActionTypes.OpenUrl,
                    Title = "OpenUrl",
                    Value = "https://dev.botframework.com/"
                }
            };
            return MessageFactory.SuggestedActions(suggestedActions, "Select and action to perform");
        }

        private bool IsBackAction(ITurnContext context)
        {
            var message = context.Activity.Text;
            return string.Equals(message, "message to bot from ImBack action")
                || string.Equals(message, "hidden message from PostBack action");
        }
    }
}
