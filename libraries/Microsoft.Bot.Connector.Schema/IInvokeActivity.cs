// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Synchronous request to invoke an operation.
    /// </summary>
    public interface IInvokeActivity : IActivity
    {
        /// <summary>
        /// Gets or sets name of the operation to invoke.
        /// </summary>
        /// <value>
        /// Name of the operation to invoke.
        /// </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets open-ended value.
        /// </summary>
        /// <value>
        /// Open-ended value.
        /// </value>
        object Value { get; set; }

        /// <summary>
        /// Gets or sets reference to another conversation or activity.
        /// </summary>
        /// <value>
        /// Reference to another conversation or activity.
        /// </value>
        ConversationReference RelatesTo { get; set; }
    }
}
