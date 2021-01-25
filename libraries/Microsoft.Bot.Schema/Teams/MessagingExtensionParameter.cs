// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Messaging extension query parameters.
    /// </summary>
    public partial class MessagingExtensionParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionParameter"/> class.
        /// </summary>
        public MessagingExtensionParameter()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionParameter"/> class.
        /// </summary>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        public MessagingExtensionParameter(string name = default(string), object value = default(object))
        {
            Name = name;
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets name of the parameter.
        /// </summary>
        /// <value>The name of the parameter.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets value of the parameter.
        /// </summary>
        /// <value>The value of the parameter.</value>
        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
