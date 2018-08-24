// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
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

        public override async Task<DialogStatus> DialogBeginAsync(DialogContext dc, DialogOptions options = null)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Start the inner dialog.
            var dialogState = new DialogState();
            dc.ActiveDialog.State[PersistedDialogState] = dialogState;
            var cdc = new DialogContext(_dialogs, dc.Context, dialogState);
            return await OnDialogBeginAsync(cdc, options).ConfigureAwait(false);
        }

        public override async Task<DialogStatus> DialogContinueAsync(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Continue execution of inner dialog.
            var dialogState = (DialogState)dc.ActiveDialog.State[PersistedDialogState];
            var cdc = new DialogContext(_dialogs, dc.Context, dialogState);
            return await OnDialogContinueAsync(cdc).ConfigureAwait(false);
        }

        public override async Task<DialogStatus> DialogResumeAsync(DialogContext dc, DialogReason reason, object result = null)
        {
            // Containers are typically leaf nodes on the stack but the dev is free to push other dialogs
            // on top of the stack which will result in the container receiving an unexpected call to
            // dialogResume() when the pushed on dialog ends.
            // To avoid the container prematurely ending we need to implement this method and simply
            // ask our inner dialog stack to re-prompt.
            return await DialogRepromptAsync(dc.Context, dc.ActiveDialog).ConfigureAwait(false);
        }

        public override async Task<DialogStatus> DialogRepromptAsync(ITurnContext turnContext, DialogInstance instance)
        {
            // Delegate to inner dialog.
            var dialogState = (DialogState)instance.State[PersistedDialogState];
            var cdc = new DialogContext(_dialogs, turnContext, dialogState);
            return await OnDialogRepromptAsync(cdc).ConfigureAwait(false);
        }

        public override async Task<DialogStatus> DialogEndAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason)
        {
            // Notify inner dialog
            var dialogState = (DialogState)instance.State[PersistedDialogState];
            var cdc = new DialogContext(_dialogs, turnContext, dialogState);
            return await OnDialogEndAsync(cdc, reason).ConfigureAwait(false);
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

        protected async Task<DialogStatus> OnDialogRunAsync(DialogContext dc)
        {
            return await dc.RunAsync().ConfigureAwait(false);
        }

        protected virtual async Task<DialogStatus> OnDialogBeginAsync(DialogContext dc, DialogOptions options)
        {
            return await dc.BeginAsync(InitialDialogId, options).ConfigureAwait(false);
        }

        protected virtual async Task<DialogStatus> OnDialogEndAsync(DialogContext dc, DialogReason reason)
        {
            if (reason == DialogReason.CancelCalled)
            {
                return await dc.CancelAllAsync().ConfigureAwait(false);
            }
            else
            {
                return DialogStatus.Complete;
            }
        }

        protected virtual async Task<DialogStatus> OnDialogContinueAsync(DialogContext dc)
        {
            var result = await dc.ContinueAsync().ConfigureAwait(false);

            if (result == DialogStatus.Cancelled)
            {
                return await dc.CancelAllAsync().ConfigureAwait(false);
            }
            else
            {
                return result;
            }
        }

        protected virtual async Task<DialogStatus> OnDialogRepromptAsync(DialogContext dc)
        {
            return await dc.RepromptAsync().ConfigureAwait(false);
        }
    }
}
