// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Client.Models
{
    /// <summary>
    /// Metadata object pertaining to an activity.
    /// </summary>
    public partial class Entity
    {
        /// <summary>
        /// Gets properties that are not otherwise defined by the <see cref="Entity"/> type but that
        /// might appear in the REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Properties { get; } = new Dictionary<string, JsonElement>();

        /// <summary>
        /// Retrieve internal payload.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <returns>T as T.</returns>
        public T GetAs<T>()
        {
            return this.ToObject<T>();
        }

        /// <summary>
        /// Set internal payload.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <param name="obj">obj.</param>
        public void SetAs<T>(T obj)
        {
            var entity = obj.ToObject<Entity>();
            Type = entity.Type;
            Properties.Clear();
            foreach (var property in entity.Properties)
            {
                Properties.Add(property.Key, property.Value);
            }
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

            return JsonSerializer.SerializeToUtf8Bytes(this, SerializationConfig.DefaultSerializeOptions)
                .SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(other, SerializationConfig.DefaultSerializeOptions));
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
