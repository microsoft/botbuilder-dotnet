// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Result returned to the caller of one of the various stack manipulation methods and used to
    /// return the result from a final call to `DialogContext.end()` to the bots logic.
    /// </summary>
    public class DialogResult
    {
        /// <summary>
        /// This will be `true` if there is still an active dialog on the stack.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Result returned by a dialog that was just ended.This will only be populated in certain
        /// cases:
        /// 
        /// - The bot calls `dc.begin()` to start a new dialog and the dialog ends immediately.
        /// - The bot calls `dc.continue()` and a dialog that was active ends.
        ///
        /// In all cases where it's populated, [active](#active) will be `false`.        
        /// </summary>
        public object Result { get; set; }
    }
}
