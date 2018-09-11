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

            _dialogs = new DialogSet();
        }

        protected string InitialDialogId { get; set; }

        public override async Task<DialogTurnResult> DialogBeginAsync(DialogContext outerDc, DialogOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (outerDc == null)
            {
                throw new ArgumentNullException(nameof(outerDc));
            }

            // Start the inner dialog.
            var dialogState = new DialogState();
            outerDc.ActiveDialog.State[PersistedDialogState] = dialogState;
            var innerDc = new DialogContext(_dialogs, outerDc.Context, dialogState);
            var turnResult = await OnDialogBeginAsync(innerDc, options, cancellationToken).ConfigureAwait(false);

            // Check for end of inner dialog
            if (turnResult.Status != DialogTurnStatus.Waiting)
            {
                // Return result to calling dialog
                return await EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Just signal waiting
                return Dialog.EndOfTurn;
            }
        }

        public override async Task<DialogTurnResult> DialogContinueAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (outerDc == null)
            {
                throw new ArgumentNullException(nameof(outerDc));
            }

            // Continue execution of inner dialog.
            var dialogState = (DialogState)outerDc.ActiveDialog.State[PersistedDialogState];
            var innerDc = new DialogContext(_dialogs, outerDc.Context, dialogState);
            var turnResult = await OnDialogContinueAsync(innerDc, cancellationToken).ConfigureAwait(false);

            if (turnResult.Status != DialogTurnStatus.Waiting)
            {
                return await EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return Dialog.EndOfTurn;
            }
        }

        public override async Task<DialogTurnResult> DialogResumeAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Containers are typically leaf nodes on the stack but the dev is free to push other dialogs
            // on top of the stack which will result in the container receiving an unexpected call to
            // dialogResume() when the pushed on dialog ends.
            // To avoid the container prematurely ending we need to implement this method and simply
            // ask our inner dialog stack to re-prompt.
            await DialogRepromptAsync(outerDc.Context, outerDc.ActiveDialog, cancellationToken).ConfigureAwait(false);
            return Dialog.EndOfTurn;
        }

        public override async Task DialogRepromptAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Delegate to inner dialog.
            var dialogState = (DialogState)instance.State[PersistedDialogState];
            var innerDc = new DialogContext(_dialogs, turnContext, dialogState);
            await innerDc.RepromptAsync(cancellationToken).ConfigureAwait(false);

            // Notify component
            await OnDialogRepromptAsync(turnContext, instance, cancellationToken).ConfigureAwait(false);
        }

        public override async Task DialogEndAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Forward cancel to inner dialogs
            if (reason == DialogReason.CancelCalled)
            {
                var dialogState = (DialogState)instance.State[PersistedDialogState];
                var innerDc = new DialogContext(_dialogs, turnContext, dialogState);
                await innerDc.CancelAllAsync(cancellationToken).ConfigureAwait(false);
            }

            await OnDialogEndAsync(turnContext, instance, reason, cancellationToken).ConfigureAwait(false);
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

        protected virtual Task<DialogTurnResult> OnDialogBeginAsync(DialogContext innerDc, DialogOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return innerDc.BeginAsync(InitialDialogId, options, cancellationToken);
        }

        protected virtual Task<DialogTurnResult> OnDialogContinueAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return innerDc.ContinueAsync(cancellationToken);
        }

        protected virtual Task OnDialogEndAsync(ITurnContext context, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDialogRepromptAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        protected virtual Task<DialogTurnResult> EndComponentAsync(DialogContext outerDc, object result, CancellationToken cancellationToken)
        {
            return outerDc.EndAsync(result);
        }
    }
}
