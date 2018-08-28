// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Base class for all dialogs.
    /// </summary>
    public abstract class Dialog
    {
        public static readonly DialogTurnResult EndOfTurn = new DialogTurnResult
        {
            HasActive = true,
            HasResult = false,
        };

        public Dialog(string dialogId)
        {
            if (string.IsNullOrWhiteSpace(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            Id = dialogId;
        }

        public string Id { get; }

        /// <summary>
        /// Method called when a new dialog has been pushed onto the stack and is being activated.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="options">(Optional) arguments that were passed to the dialog during `begin()` call that started the instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task<DialogTurnResult> DialogBeginAsync(DialogContext dc, DialogOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Method called when an instance of the dialog is the "current" dialog and the
        /// user replies with a new activity. The dialog will generally continue to receive the users
        /// replies until it calls either `DialogSet.end()` or `DialogSet.begin()`.
        /// If this method is NOT implemented then the dialog will automatically be ended when the user replies.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task<DialogTurnResult> DialogContinueAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // By default just end the current dialog.
            return await dc.EndAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Method called when an instance of the dialog is being returned to from another
        /// dialog that was started by the current instance using `DialogSet.begin()`.
        /// If this method is NOT implemented then the dialog will be automatically ended with a call
        /// to `DialogSet.endDialogWithResult()`. Any result passed from the called dialog will be passed
        /// to the current dialogs parent.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="reason">Reason why the dialog resumed.</param>
        /// <param name="result">(Optional) value returned from the dialog that was called. The type of the value returned is dependant on the dialog that was called.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task<DialogTurnResult> DialogResumeAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // By default just end the current dialog and return result to parent.
            return await dc.EndAsync(result, cancellationToken).ConfigureAwait(false);
        }

        public virtual Task DialogRepromptAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            // No-op by default
            return Task.CompletedTask;
        }

        public virtual Task DialogEndAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            // No-op by default
            return Task.CompletedTask;
        }
    }
}
