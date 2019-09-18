// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Result returned to the caller of one of the various stack manipulation methods.
    /// </summary>
    /// <remarks>
    /// Use <see cref="DialogContext.EndDialogAsync(object, System.Threading.CancellationToken)"/>
    /// to end a <see cref="Dialog"/> and return a result to the calling context.
    /// </remarks>
    public class DialogTurnResult
    {
        public DialogTurnResult(DialogTurnStatus status, object result = null)
        {
            Status = status;
            Result = result;
        }

        /// <summary>
        /// Gets or sets the current status of the stack.
        /// </summary>
        /// <value>
        /// The current status of the stack.
        /// </value>
        public DialogTurnStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the result returned by a dialog that was just ended.
        /// This will only be populated in certain cases:
        ///
        /// - The bot calls `DialogContext.BeginDialogAsync()` to start a new dialog and the dialog ends immediately.
        /// - The bot calls `DialogContext.ContinueDialogAsync()` and a dialog that was active ends.
        ///
        /// In all cases where it's populated, <see cref="DialogContext.ActiveDialog"/> will be `null`.
        /// </summary>
        /// <value>
        /// The result returned by a dialog that was just ended.
        /// </value>
        public object Result { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a DialogCommand has ended its parent container and the parent should not perform any further processing.
        /// </summary>
        /// <value>
        /// Whether a DialogCommand has ended its parent container and the parent should not perform any further processing.
        /// </value>
        public bool ParentEnded { get; set; }
    }
}
