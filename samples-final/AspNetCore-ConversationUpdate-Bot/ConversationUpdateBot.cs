// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AspNetCore_ConversationUpdate_Bot
{
    public class ConversationUpdateBot : IBot
    {
        public Task OnTurn(ITurnContext context)
        {
            switch (context.Activity.Type)
            {
                // On "conversationUpdate"-type activities this bot will send a greeting message to users joining the conversation.
                case ActivityTypes.ConversationUpdate:
                    var newUserName = context.Activity.MembersAdded.FirstOrDefault()?.Name;
                    if (!string.IsNullOrWhiteSpace(newUserName) && newUserName != "Bot")
                    {
                        return context.SendActivity($"Hello {newUserName}!");
                    }

                    break;
                case ActivityTypes.Message:
                    return context.SendActivity("Welcome to the conversationUpdate-bot! " +
                        "On a _\"conversationUpdate\"_-type activity, this bot will greet new users.");
            }
            return Task.CompletedTask;
        }
    }
}
