// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Tracking information for a dialog on the stack.
    /// </summary>
    public class DialogInstance
    {
        /// <summary>
        /// ID of the dialog this instance is for.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The instances persisted state.
        /// </summary>
        public object State { get; set; }
    }
}
