// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

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
        public IDictionary<string, object> State { get; set; }

        /// <summary>
        /// Used when the instance is a Waterfall.
        /// </summary>
        public int Step { get; set; }
    }
}
