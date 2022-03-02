// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Message reaction object.
    /// </summary>
    public partial class MessageReaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReaction"/> class.
        /// </summary>
        public MessageReaction()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReaction"/> class.
        /// </summary>
        /// <param name="type">Message reaction type. Possible values include:
        /// 'like', 'plusOne'.</param>
        public MessageReaction(string type = default)
        {
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets message reaction type. Possible values include:
        /// 'like', 'plusOne'.
        /// </summary>
        /// <value>The message reaction type.</value>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
