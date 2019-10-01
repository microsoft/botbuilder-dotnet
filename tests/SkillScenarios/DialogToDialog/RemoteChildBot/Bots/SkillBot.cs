// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Skills.Gaps;
using Microsoft.Bot.Schema;

namespace RemoteChildBot.Bots
{
    public class SkillBot<T> : IBot
        where T : Dialog
    {
        public SkillBot(ConversationState conversationState, T dialog)
        {
            ConversationState = conversationState;
            Dialog = dialog;
        }

        protected BotState ConversationState { get; }

        protected Dialog Dialog { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            // Run the Dialog with the new message Activity.
            var result = await Dialog.InvokeAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);

            if (result.Status == DialogTurnStatus.Complete)
            {
                // Send End of conversation at the end.
                var activity = new Activity(ActivityTypes.EndOfConversation)
                {
                    Value = result.Result,
                };
                await turnContext.SendActivityAsync(activity, cancellationToken);
            }

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
