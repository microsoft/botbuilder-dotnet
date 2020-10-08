// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Possible assignment of an entity to a property.
    /// </summary>
    public class EntityAssignment
    {
        /// <summary>
        /// Gets or sets event name.
        /// </summary>
        /// <value>Event name to surface.</value>
        [JsonProperty("event")]
        public string Event { get; set; }

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
        /// Gets or sets an alternative assignment.
        /// </summary>
        /// <value>Alternative assignment.</value>
        [JsonProperty("alternative")]
        public EntityAssignment Alternative { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this entity was in <see cref="DialogPath.ExpectedProperties"/>.
        /// </summary>
        /// <value>True if entity is expected.</value>
        [JsonProperty("isExpected")]
        public bool IsExpected { get; set; }

        /// <summary>
        /// Gets or sets the number of times event has been raised.
        /// </summary>
        /// <value>
        /// The number of times event has been raised.
        /// </value>
        [JsonProperty("raisedCount")]
        public uint RaisedCount { get; set; } = 0;

        /// <summary>
        /// Gets the alternative entity assignments.
        /// </summary>
        /// <value>
        /// The alternative entity assignments.
        /// </value>
        [JsonIgnore]
        public IEnumerable<EntityAssignment> Alternatives
        {
            get
            {
                var current = this;
                do
                {
                    yield return current;
                    current = current.Alternative;
                } 
                while (current != null);
            }
        }

        /// <summary>
        /// Add alternatives to a single assignment.
        /// </summary>
        /// <param name="alternatives">Alternatives to add.</param>
        public void AddAlternatives(IEnumerable<EntityAssignment> alternatives)
        {
            var current = this;
            Alternative = null;
            foreach (var alternative in alternatives)
            {
                if (alternative != this)
                {
                    current.Alternative = alternative;
                    current = alternative;
                }
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
            => (IsExpected ? "+" : string.Empty) + $"{Event}: {Property} = {Operation}({Entity})";
    }
}
