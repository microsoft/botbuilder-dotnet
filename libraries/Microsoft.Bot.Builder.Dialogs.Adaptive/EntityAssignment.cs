// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Possible assignment of an entity to a property.
    /// </summary>
    public class EntityAssignment
    {
        /// <summary>
        /// Gets or sets name of property being assigned.
        /// </summary>
        /// <value>Property being assigned.</value>
        [JsonProperty("property")]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets operation to assign entity to property.
        /// </summary>
        /// <value>Operation to assign entity to property.</value>
        [JsonProperty("operation")]
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets entity being assigned.
        /// </summary>
        /// <value>Entity being assigned.</value>
        [JsonProperty("entity")]
        public EntityInfo Entity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this entity was in <see cref="DialogPath.ExpectedProperties"/>.
        /// </summary>
        /// <value>True if entity is expected.</value>
        [JsonProperty("isExpected")]
        public bool IsExpected { get; set; }

        public override string ToString()
            => (IsExpected ? "+" : string.Empty) + $"{Property} = {Operation}({Entity})";
    }
}
