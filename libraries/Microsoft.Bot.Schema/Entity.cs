// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Metadata object pertaining to an activity
    /// </summary>
    public partial class Entity
    {
        /// <summary>
        /// Initializes a new instance of the Entity class.
        /// </summary>
        public Entity()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Entity class.
        /// </summary>
        /// <param name="type">Type of this entity (RFC 3987 IRI)</param>
        public Entity(string type = default(string))
        {
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets type of this entity (RFC 3987 IRI)
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

    }
}
