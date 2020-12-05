// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Thing (entity type: "https://schema.org/Thing").
    /// </summary>
    public partial class Thing
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
        public Thing(string type = default(string), string name = default(string))
        {
            Type = type;
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the type of the thing.
        /// </summary>
        /// <value>The type of the thing.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the thing.
        /// </summary>
        /// <value>The name of the thing.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
