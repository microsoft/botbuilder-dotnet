// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// How to modify an action sequence.
    /// </summary>
    public enum ActionChangeType
    {
        /// <summary>
        /// Add the change actions to the head of the sequence.
        /// </summary>
        InsertActions,

        /// <summary>
        /// Add the changeactions to the tail of the sequence.
        /// </summary>
        AppendActions,

        /// <summary>
        /// Terminate the action sequence.
        /// </summary>
        EndSequence,

        /// <summary>
        /// Terminate the action sequence, then add the change actions.
        /// </summary>
        ReplaceSequence,
    }
}
