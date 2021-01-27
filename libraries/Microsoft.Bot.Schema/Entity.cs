// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Metadata object pertaining to an activity.
    /// </summary>
    public class Entity
    {
        /// <summary>Initializes a new instance of the <see cref="Entity"/> class.</summary>
        public Entity()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Entity"/> class.</summary>
        /// <param name="type">Type of this entity (RFC 3987 IRI).</param>
        public Entity(string type = default(string))
        {
            Type = type;
        }

        /// <summary>
        /// Gets or sets type of this entity (RFC 3987 IRI).
        /// </summary>
        /// <value>The type of this entity.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets properties that are not otherwise defined by the <see cref="Entity"/> type but that
        /// might appear in the REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData(ReadData = true, WriteData = true)]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public JObject Properties { get; set; } = new JObject();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Retrieve internal payload.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <returns>T as T.</returns>
        public T GetAs<T>()
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(this));
        }

        /// <summary>
        /// Set internal payload.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <param name="obj">obj.</param>
        public void SetAs<T>(T obj)
        {
            var entity = JsonConvert.DeserializeObject<Entity>(JsonConvert.SerializeObject(obj));
            this.Type = entity.Type;
            this.Properties = entity.Properties;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">The other object to compair against.</param>
        /// <returns>true if the current object is equal to the other parameter, otherwise false.</returns>
        public bool Equals(Entity other)
        {
            if (other == null)
            {
                return false;
            }

            return JsonConvert.SerializeObject(this).Equals(JsonConvert.SerializeObject(other), StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the specifid object is equal to the current object.
        /// </summary>
        /// <param name="obj">The other object to compair against.</param>
        /// <returns>true if the current object is equal to the obj parameter, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals(obj as Entity);
        }

        /// <summary>
        /// Hash function that generates a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
