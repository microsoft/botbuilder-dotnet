// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Base class for all dialogs.
    /// </summary>
    public abstract class Dialog
    {
        private string _id;

        public static readonly DialogTurnResult EndOfTurn = new DialogTurnResult
        {
            HasActive = true,
            HasResult = false,
        };

        public Dialog(string dialogId)
        {
            this._id = dialogId;
        }

        public string Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Method called when a new dialog has been pushed onto the stack and is being activated.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="options">(Optional) arguments that were passed to the dialog during `begin()` call that started the instance.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task<DialogTurnResult> DialogBeginAsync(DialogContext dc, DialogOptions options = null);

        /// <summary>
        /// Method called when an instance of the dialog is the "current" dialog and the
        /// user replies with a new activity. The dialog will generally continue to receive the users
        /// replies until it calls either `DialogSet.end()` or `DialogSet.begin()`.
        /// If this method is NOT implemented then the dialog will automatically be ended when the user replies.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async virtual Task<DialogTurnResult> DialogContinueAsync(DialogContext dc)
        {
            // By default just end the current dialog.
            return await dc.EndAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Method called when an instance of the dialog is being returned to from another
        /// dialog that was started by the current instance using `DialogSet.begin()`.
        /// If this method is NOT implemented then the dialog will be automatically ended with a call
        /// to `DialogSet.endDialogWithResult()`. Any result passed from the called dialog will be passed
        /// to the current dialogs parent.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="result">(Optional) value returned from the dialog that was called. The type of the value returned is dependant on the dialog that was called.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async virtual Task<DialogTurnResult> DialogResumeAsync(DialogContext dc, DialogReason reason, object result)
        {
            // By default just end the current dialog and return result to parent.
            return await dc.EndAsync(result).ConfigureAwait(false);
        }

        public virtual Task DialogRepromptAsync(ITurnContext context, DialogInstance instance)
        {
            // No-op by default
            return Task.CompletedTask;
        }

        public virtual Task DialogEndAsync(ITurnContext context, DialogInstance instance, DialogReason reason)
        {
            // No-op by default
            return Task.CompletedTask;
        }
    }
}
