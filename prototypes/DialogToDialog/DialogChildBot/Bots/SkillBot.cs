// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore.Internal;

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

            if (turnContext.Activity.Type == ActivityTypes.EndOfConversation)
            {
                if (dialogContext.Stack.Any())
                {
                    // Handle remote cancellation request if we have something in the stack.
                    var activeDialogContext = GetActiveDialogContext(dialogContext);

                    // Send cancellation message to the top dialog in the stack to ensure all the parents are cancelled in the right order. 
                    await activeDialogContext.CancelAllDialogsAsync(true, cancellationToken: cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text("**SkillBot.** The current dialog in the skill was **cancelled** by a request **from the host**, do some cleanup if needed here"), cancellationToken);
                }
            }
            else
            {
                // Run the Dialog with the new message Activity and capture the results so we can send end of conversation if needed.
                var result = await dialogContext.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
                if (result.Status == DialogTurnStatus.Empty)
                {
                    result = await dialogContext.BeginDialogAsync(Dialog.Id, null, cancellationToken).ConfigureAwait(false);
                }

                // Send end of conversation if it is complete
                if (result.Status == DialogTurnStatus.Complete)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"**SkillBot.** The dialog in the skill has **completed**. Sending EndOfConversation"), cancellationToken);

                    // Send End of conversation at the end.
                    var activity = new Activity(ActivityTypes.EndOfConversation) { Value = result.Result };
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
                else if (result.Status == DialogTurnStatus.Cancelled)
                {
                    // TODO: this code doesn't seem to be called. Check with Steve
                    await turnContext.SendActivityAsync(MessageFactory.Text("**SkillBot.** The current dialog in the skill was **cancelled from the skill** code. . Sending EndOfConversation"), cancellationToken);
                    
                    // Send End of conversation at the end.
                    var activity = new Activity(ActivityTypes.EndOfConversation) { Value = result.Result };
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        // Recursively walk up the DC stack to find the active DC.
        private DialogContext GetActiveDialogContext(DialogContext dialogContext)
        {
            var child = dialogContext.Child;
            if (child == null)
            {
                return dialogContext;
            }

            return GetActiveDialogContext(child);
        }
    }
}
