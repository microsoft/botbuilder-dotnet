// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Result returned to the caller of one of the various stack manipulation methods and used to
    /// return the result from a final call to `DialogContext.end()` to the bots logic.
    /// </summary>
    public class DialogCompletion
    {
        /// <summary>
        /// If 'true' the dialog is still active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// If 'true' the dialog just completed and the final [result](#result) can be retrieved.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Result returned by a dialog that was just ended.This will only be populated in certain
        /// cases:
        /// 
        /// - The bot calls `dc.begin()` to start a new dialog and the dialog ends immediately.
        /// - The bot calls `dc.continue()` and a dialog that was active ends.
        ///
        /// In all cases where it's populated, [active](#active) will be `false`.        
        /// </summary>
        public IDictionary<string, object> Result { get; set; }
    }
}
