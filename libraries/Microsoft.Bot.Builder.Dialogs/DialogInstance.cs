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
        /// Gets or sets the ID of the dialog this instance is for.
        /// </summary>
        /// <value>
        /// ID of the dialog this instance is for.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the instances persisted state.
        /// </summary>
        /// <value>
        /// The instances persisted state.
        /// </value>
        public IDictionary<string, object> State { get; set; }

        /// <summary>
        /// Gets or sets the step used when the instance is a Waterfall.
        /// </summary>
        /// <value>
        /// The step used when the instance is a Waterfall.
        /// </value>
        public int Step { get; set; }
    }
}
