// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class ComponentDialog : Dialog
    {
        private const string PersistedDialogState = "dialogs";

        private DialogSet _dialogs;

        public ComponentDialog(string dialogId)
            : base(dialogId)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            _dialogs = new DialogSet(null);
        }

        protected string InitialDialogId { get; set; }

        public override async Task<DialogTurnResult> DialogBeginAsync(DialogContext dc, DialogOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Start the inner dialog.
            var dialogState = new DialogState();
            dc.ActiveDialog.State[PersistedDialogState] = dialogState;
            var cdc = new DialogContext(_dialogs, dc.Context, dialogState);
            var turnResult = await OnDialogBeginAsync(cdc, options, cancellationToken).ConfigureAwait(false);

            // Check for end of inner dialog
            if (turnResult.Result != null)
            {
                // Return result to calling dialog
                return await dc.EndAsync(turnResult.Result, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Just signal waiting
                return Dialog.EndOfTurn;
            }
        }

        public override async Task<DialogTurnResult> DialogContinueAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Continue execution of inner dialog.
            var dialogState = (DialogState)dc.ActiveDialog.State[PersistedDialogState];
            var cdc = new DialogContext(_dialogs, dc.Context, dialogState);
            return await OnDialogContinueAsync(cdc, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> DialogResumeAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Containers are typically leaf nodes on the stack but the dev is free to push other dialogs
            // on top of the stack which will result in the container receiving an unexpected call to
            // dialogResume() when the pushed on dialog ends.
            // To avoid the container prematurely ending we need to implement this method and simply
            // ask our inner dialog stack to re-prompt.
            await DialogRepromptAsync(dc.Context, dc.ActiveDialog, cancellationToken).ConfigureAwait(false);
            return Dialog.EndOfTurn;
        }

        public override async Task DialogRepromptAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Delegate to inner dialog.
            var dialogState = (DialogState)instance.State[PersistedDialogState];
            var cdc = new DialogContext(_dialogs, turnContext, dialogState);
            await OnDialogRepromptAsync(cdc, cancellationToken).ConfigureAwait(false);
        }

        public override async Task DialogEndAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Notify inner dialog
            var dialogState = (DialogState)instance.State[PersistedDialogState];
            var cdc = new DialogContext(_dialogs, turnContext, dialogState);
            await OnDialogEndAsync(cdc, reason, cancellationToken).ConfigureAwait(false);
        }

        protected Dialog AddDialog(Dialog dialog)
        {
            _dialogs.Add(dialog);
            if (string.IsNullOrEmpty(InitialDialogId))
            {
                InitialDialogId = dialog.Id;
            }

            return dialog;
        }

        protected virtual async Task<DialogTurnResult> OnDialogBeginAsync(DialogContext dc, DialogOptions options, CancellationToken cancellationToken = default(CancellationToken)) => await dc.BeginAsync(InitialDialogId, options, cancellationToken).ConfigureAwait(false);

        protected virtual async Task<DialogTurnResult> OnDialogEndAsync(DialogContext dc, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (reason == DialogReason.CancelCalled)
            {
                await dc.CancelAllAsync(cancellationToken).ConfigureAwait(false);
                return new DialogTurnResult(DialogTurnStatus.Cancelled);
            }

            if (dc.ActiveDialog != null)
            {
                await this.OnDialogRepromptAsync(dc).ConfigureAwait(false);
                return Dialog.EndOfTurn;
            }
            else
            {
                return new DialogTurnResult(DialogTurnStatus.Complete);
            }
        }

        protected virtual async Task<DialogTurnResult> OnDialogContinueAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // runs next step
            var result = await dc.ContinueAsync(cancellationToken).ConfigureAwait(false);

            if (result.Status == DialogTurnStatus.Complete || result.Status == DialogTurnStatus.Cancelled)
            {
                // ends current dialog, and if there is another on the stack, throws the result away
                return await dc.EndAsync(result.Result).ConfigureAwait(false);
            }

            return result;
        }

        protected virtual async Task OnDialogRepromptAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken)) => await dc.RepromptAsync(cancellationToken).ConfigureAwait(false);
    }
}
