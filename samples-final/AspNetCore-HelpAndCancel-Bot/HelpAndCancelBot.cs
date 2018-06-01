// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Core.Extensions;

namespace AspNetCore_HelpAndCancel_Bot
{
    public class HelpAndCancelBot : IBot
    {
        public Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                var newUserName = context.Activity.MembersAdded.FirstOrDefault()?.Name;
                if (!string.Equals("Bot", newUserName))
                {
                    return context.SendActivity("Welcome to the HelpAndCancel-bot!");
                }
            }
            else if (context.Activity.Type == ActivityTypes.Message)
            {
                var message = context.Activity.Text;
                var messageState = context.GetConversationState<MessageState>();

                if (string.Equals("help", message, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    if (messageState.LastNumber.HasValue)
                    {
                        return context.SendActivity($"Just type the number that follows {messageState.LastNumber}. Or type _'cancel'_ to start a new count");
                    }

                    return context.SendActivity("Type a number and count with me");
                }
                else if (string.Equals("cancel", message, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    if (messageState.LastNumber.HasValue)
                    {
                        messageState.LastNumber = null;
                        return context.SendActivity("Ok, canceling this iteration...");
                    }

                    return context.SendActivity("Sorry, nothing to cancel");
                }
                
                if (messageState.LastNumber.HasValue)
                {
                    if (int.TryParse(message, out int userNumber))
                    {
                        if (userNumber == messageState.LastNumber + 1)
                        {
                            messageState.LastNumber = userNumber + 1;
                            return context.SendActivity(messageState.LastNumber.ToString());
                        }

                        return context.SendActivity($"Please, type the number that follows {messageState.LastNumber}");
                    }

                    return context.SendActivity($"Please, type just a number");
                }

                if (int.TryParse(message, out int result))
                {
                    messageState.LastNumber = result + 1;
                    return context.SendActivity($"starting with {messageState.LastNumber}");
                }

                return context.SendActivity($"Please, type just a number");
            }

            return Task.CompletedTask;
        }
    }
}
