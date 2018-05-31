// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Interface for all Dialog objects that can be added to a `DialogSet`. The dialog should generally
    /// be a singleton and added to a dialog set using `DialogSet.add()` at which point it will be 
    /// assigned a unique ID.
    /// </summary>
    public interface IDialog
    {
        /// <summary>
        /// Method called when a new dialog has been pushed onto the stack and is being activated.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="dialogArgs">(Optional) arguments that were passed to the dialog during `begin()` call that started the instance.</param>  
        Task DialogBegin(DialogContext dc, IDictionary<string, object> dialogArgs = null);
    }
}
