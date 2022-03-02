// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;

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
        /// <param name="key">The key for this Fact.</param>
        /// <param name="value">The value for this Fact.</param>
        public Fact(string key = default, string value = default)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the key for this Fact.
        /// </summary>
        /// <value>The key for this fact.</value>
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value for this Fact.
        /// </summary>
        /// <value>The value for this Fact.</value>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
