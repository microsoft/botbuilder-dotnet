// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace DialogChildBot.Bots
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
            // TODO: replace this with DialogManager
            var dialogSet = new DialogSet(ConversationState.CreateProperty<DialogState>("DialogState")) { TelemetryClient = Dialog.TelemetryClient };
            dialogSet.Add(Dialog);

            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken).ConfigureAwait(false);

            // Run the Dialog with the new message Activity and capture the results so we can send end of conversation if needed.
            var result = await dialogContext.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
            if (result.Status == DialogTurnStatus.Empty)
            {
                result = await dialogContext.BeginDialogAsync(Dialog.Id, null, cancellationToken).ConfigureAwait(false);
            }

            // Send end of conversation if it is complete
            if (result.Status == DialogTurnStatus.Complete || result.Status == DialogTurnStatus.Cancelled) 
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Skill dialog completed sending EndOfConversation"), cancellationToken);

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
