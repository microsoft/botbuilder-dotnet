// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Result returned to the caller of one of the various stack manipulation methods and used to
    /// return the result from a final call to `DialogContext.end()` to the bots logic.
    /// </summary>
    public class DialogTurnResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the dialog is still active.
        /// </summary>
        /// <value>
        /// <c>true</c> the dialog is still active; otherwise <c>false</c>.
        /// </value>
        public bool HasActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog completed and its final [result](#result) can be retrieved.
        /// </summary>
        /// <value>
        /// <c>true</c> if the dialog just completed and the final [result](#result) can be retrieved; otherwise <c>false</c>.
        /// </value>
        public bool HasResult { get; set; }

        /// <summary>
        /// Gets or sets the result returned by a dialog that was just ended.
        /// This will only be populated in certain cases:
        ///
        /// - The bot calls `dc.begin()` to start a new dialog and the dialog ends immediately.
        /// - The bot calls `dc.continue()` and a dialog that was active ends.
        ///
        /// In all cases where it's populated, [active](#active) will be `false`.
        /// </summary>
        /// <value>
        /// The result returned by a dialog that was just ended.
        /// </value>
        public object Result { get; set; }
    }
}
