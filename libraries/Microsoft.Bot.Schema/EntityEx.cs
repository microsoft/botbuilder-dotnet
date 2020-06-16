// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Instance of the <see cref="Entity"/> Class.
    /// </summary>
    public partial class Entity : IEquatable<Entity>
    {
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

        public bool Equals(Entity other)
        {
            if (other == null)
            {
                return false;
            }

            return JsonConvert.SerializeObject(this).Equals(JsonConvert.SerializeObject(other), StringComparison.Ordinal);
        }

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

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
