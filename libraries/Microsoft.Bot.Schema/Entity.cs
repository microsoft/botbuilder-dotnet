// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Metadata object pertaining to an activity.
    /// </summary>
    public partial class Entity
    {
        /// <summary>Initializes a new instance of the <see cref="Entity"/> class.</summary>
        public Entity()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="Entity"/> class.</summary>
        /// <param name="type">Type of this entity (RFC 3987 IRI).</param>
        public Entity(string type = default(string))
        {
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets type of this entity (RFC 3987 IRI).
        /// </summary>
        /// <value>The type of this entity.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
