// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Interface Dialog objects that can be continued.
    /// </summary>
    public interface IDialogContinue : IDialog
    {
        /// <summary>
        /// Method called when an instance of the dialog is the "current" dialog and the 
        /// user replies with a new activity. The dialog will generally continue to receive the users 
        /// replies until it calls either `DialogSet.end()` or `DialogSet.begin()`.
        /// If this method is NOT implemented then the dialog will automatically be ended when the user replies.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        Task DialogContinue(DialogContext dc);
    }
}
