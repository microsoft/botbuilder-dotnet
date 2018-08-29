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

                    // Both the bot and the user will be in Activity.MembersAdded.  We only want to send a message to the user.
                    // The user is not the recipient, the bot is in this case. This is what we are checking for here.
                    if (context.Activity.MembersAdded.Any(member => member.Id != context.Activity.Recipient.Id))
                    {
                        var newUserName = context.Activity.MembersAdded.FirstOrDefault()?.Name;
                        
                        if (!string.IsNullOrWhiteSpace(newUserName) && newUserName != "Bot")
                        {
                            // Greet new user by name if name exists.
                            return context.SendActivity($"Hello {newUserName}!");
                        }
                        // If no name use this default message.
                        return context.SendActivity($"Welcome to the conversationUpdate-bot!");
                    }

                    break;
                case ActivityTypes.Message:
                    return context.SendActivity("This is the conversationUpdate-bot!  " +
                                                "On a _\"conversationUpdate\"_-type activity, this bot will greet new users.");
            }

            return Task.CompletedTask;
        }
    }
}
