// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Set of key-value pairs. Advantage of this section is that key and value
    /// properties will be
    /// rendered with default style information with some delimiter between
    /// them. So there is no need for developer to specify style information.
    /// </summary>
    public class Fact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Fact"/> class.
        /// </summary>
        public Fact()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fact"/> class.
        /// </summary>
        /// <param name="key">The key for this Fact.</param>
        /// <param name="value">The value for this Fact.</param>
        public Fact(string key = default, string value = default)
        {
            Key = key;
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the key for this Fact.
        /// </summary>
        /// <value>The key for this fact.</value>
        [JsonPropertyName("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value for this Fact.
        /// </summary>
        /// <value>The value for this Fact.</value>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
