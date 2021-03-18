// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// The value field of a <see cref="ICommandResultActivity"/> contains metadata related to a command result.
    /// An optional extensible data payload may be included if defined by the command result activity name. 
    /// The presence of an error field indicates that the original command failed to complete.
    /// </summary>
    /// <typeparam name="T">Type for data field.</typeparam>
    public class CommandResultValue<T>
    {
        /// <summary>
        /// Gets or sets the id of the command.
        /// </summary>
        /// <value>
        /// Id of the command.
        /// </value>
        [JsonProperty(PropertyName = "commandId")]
        public string CommandId { get; set; }

        /// <summary>
        /// Gets or sets the data field containing optional parameters specific to this command result activity,
        /// as defined by the name. The value of the data field is a complex type.
        /// </summary>
        /// <value>
        /// Open-ended value.
        /// </value>
        [JsonProperty(PropertyName = "data")]
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets the optional error, if the command result indicates a failure.
        /// </summary>
        /// <value>
        /// Error which occurred during processing of the command.
        /// </value>
        [JsonProperty(PropertyName = "error")]
        public Error Error { get; set; }
    }
}
