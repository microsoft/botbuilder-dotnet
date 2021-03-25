// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Asynchronous external command.
    /// </summary>
    public interface ICommandActivity : IActivity
    {
        /// <summary>
        /// Gets or sets name of the command.
        /// </summary>
        /// <value>
        /// Name of the event.
        /// </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the command value.  This will be of type <see cref="CommandValue{T}"/>.
        /// </summary>
        /// <value>
        /// Value for this command.
        /// </value>
        object Value { get; set; }
    }
}
