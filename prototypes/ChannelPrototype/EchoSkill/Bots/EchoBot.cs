// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace EchoSkill.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text.Contains("end") || turnContext.Activity.Text.Contains("stop"))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"drop mike"), cancellationToken);

                // Unknown = "unknown";
                // CompletedSuccessfully = "completedSuccessfully";
                // UserCancelled = "userCancelled";
                // BotTimedOut = "botTimedOut";
                // BotIssuedInvalidMessage = "botIssuedInvalidMessage";
                // ChannelFailed = "channelFailed";
                var endOfConversation = Activity.CreateEndOfConversationActivity();
                endOfConversation.Code = EndOfConversationCodes.CompletedSuccessfully;
                await turnContext.SendActivityAsync(endOfConversation, cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Echo : {turnContext.Activity.Text}"), cancellationToken);
            }
        }
    }
}
