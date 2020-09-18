// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Represents the result of the Dialog Manager turn.
    /// </summary>
    public class DialogManagerResult
    {
        /// <summary>
        /// Gets or sets the result returned to the caller.
        /// </summary>
        /// <value>The result returned to the caller.</value>
        public DialogTurnResult TurnResult { get; set; }

        /// <summary>
        /// Gets or sets the array of resulting activities.
        /// </summary>
        /// <value>The array of resulting activities.</value>
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public Activity[] Activities { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets or sets the resulting new state.
        /// </summary>
        /// <value>The resulting new state.</value>
        public PersistedState NewState { get; set; }
    }
}
