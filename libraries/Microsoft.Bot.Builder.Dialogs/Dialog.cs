// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Abstract class for all Dialog objects that can be added to a `DialogSet`. The dialog should generally
    /// be a singleton and added to a dialog set using `DialogSet.add()` at which point it will be 
    /// assigned a unique ID.
    /// </summary>
    public abstract class Dialog
    {
        /// <summary>
        /// Method called when a new dialog has been pushed onto the stack and is being activated.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="dialogArgs">(Optional) arguments that were passed to the dialog during `begin()` call that started the instance.</param>  
        public abstract Task DialogBegin(DialogContext dc, object dialogArgs = null);

        /// <summary>
        /// Indicates whether the class supports DialogContinue
        /// </summary>
        public abstract bool HasDialogContinue { get; }

        /// <summary>
        /// (Optional) method called when an instance of the dialog is the "current" dialog and the 
        /// user replies with a new activity. The dialog will generally continue to receive the users 
        /// replies until it calls either `DialogSet.end()` or `DialogSet.begin()`.
        /// If this method is NOT implemented then the dialog will automatically be ended when the user replies.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        public virtual Task DialogContinue(DialogContext dc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Indicates whether the class supports DialogResume
        /// </summary>
        public abstract bool HasDialogResume { get; }

        /// <summary>
        /// (Optional) method called when an instance of the dialog is being returned to from another
        /// dialog that was started by the current instance using `DialogSet.begin()`.
        /// If this method is NOT implemented then the dialog will be automatically ended with a call
        /// to `DialogSet.endDialogWithResult()`. Any result passed from the called dialog will be passed
        /// to the current dialogs parent.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="result">(Optional) value returned from the dialog that was called. The type of the value returned is dependant on the dialog that was called.</param>
        public virtual Task DialogResume(DialogContext dc, object result)
        {
            throw new NotImplementedException();
        }
    }
}
