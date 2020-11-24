// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Message reaction object
    /// </summary>
    public partial class MessageReaction
    {
        /// <summary>
        /// Initializes a new instance of the MessageReaction class.
        /// </summary>
        public MessageReaction()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MessageReaction class.
        /// </summary>
        /// <param name="type">Message reaction type. Possible values include:
        /// 'like', 'plusOne'</param>
        public MessageReaction(string type = default(string))
        {
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets message reaction type. Possible values include:
        /// 'like', 'plusOne'
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

    }
}
