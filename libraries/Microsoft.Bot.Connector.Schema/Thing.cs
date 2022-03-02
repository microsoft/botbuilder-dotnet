// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Thing (entity type: "https://schema.org/Thing").
    /// </summary>
    public class Thing
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Thing"/> class.
        /// </summary>
        public Thing()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Thing"/> class.
        /// </summary>
        /// <param name="type">The type of the thing.</param>
        /// <param name="name">The name of the thing.</param>
        public Thing(string type = default, string name = default)
        {
            Type = type;
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the type of the thing.
        /// </summary>
        /// <value>The type of the thing.</value>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the thing.
        /// </summary>
        /// <value>The name of the thing.</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
