// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Asynchronous external command result.
    /// </summary>
    public interface ICommandResultActivity : IActivity
    {
        /// <summary>
        /// Gets or sets name of the command result.
        /// </summary>
        /// <value>
        /// Name of the event.
        /// </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets command result value.  This will be of type <see cref="CommandResultValue{T}"/>.
        /// </summary>
        /// <value>
        /// Open-ended value.
        /// </value>
        object Value { get; set; }
    }
}
