// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// The value field of a <see cref="ICommandActivity"/> contains metadata related to a command.
    /// An optional extensible data payload may be included if defined by the command activity name.
    /// </summary>
    /// <typeparam name="T">Type for Data feild.</typeparam>
    public class CommandValue<T>
    {
        /// <summary>
        /// Gets or sets the id of the command.
        /// </summary>
        /// <value>
        /// Id of the command.
        /// </value>
        [JsonPropertyName("commandId")]
        public string CommandId { get; set; }

        /// <summary>
        /// Gets or sets the data field containing optional parameters specific to this command activity,
        /// as defined by the name. The value of the data field is a complex type.
        /// </summary>
        /// <value>
        /// Open-ended value.
        /// </value>
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
