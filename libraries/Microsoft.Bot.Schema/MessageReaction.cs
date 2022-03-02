// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;

    /// <summary>
    /// Message reaction object.
    /// </summary>
    public class MessageReaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReaction"/> class.
        /// </summary>
        /// <param name="type">Message reaction type. Possible values include:
        /// 'like', 'plusOne'.</param>
        public MessageReaction(string type = default)
        {
            Type = type;
        }

        /// <summary>
        /// Gets or sets message reaction type. Possible values include:
        /// 'like', 'plusOne'.
        /// </summary>
        /// <value>The message reaction type.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}
