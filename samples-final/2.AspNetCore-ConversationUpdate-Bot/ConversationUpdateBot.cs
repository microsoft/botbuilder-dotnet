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
                // If Activity has an ActivityType of ConversationUpdate,
                // this bot will send a greeting message to users joining the conversation.
                case ActivityTypes.ConversationUpdate:
                    
                    //If members were added, send a welcome message to every member added that is not the bot.
                    if (context.Activity.MembersAdded.Any())
                    {
                        foreach (var member in context.Activity.MembersAdded)
                        {
                            var newUserName = member.Name;

                            if (member.Id != context.Activity.Recipient.Id)
                            {
                                context.SendActivity($"Hello {newUserName}!");
                            }
                        }
                    }

                    break;
                case ActivityTypes.Message:

                    // If Activity has an ActivityType of Message this bot will reply with this message.
                    return context.SendActivity("This is the conversationUpdate-bot!  " +
                                                "On a _\"conversationUpdate\"_-type activity, this bot will greet new users.");
            }

            return Task.CompletedTask;
        }
    }
}
