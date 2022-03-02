// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Extensions for converting objects to desired types using serialization.
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Decompose an object into its constituent JSON elements.
        /// </summary>
        /// <param name="value">The object to be decomposed into JSON elements.</param>
        /// <returns>A dictionary of JSON elements keyed by property name.</returns>
        public static Dictionary<string, JsonElement> ToJsonElements(this object value)
        {
            var elements = new Dictionary<string, JsonElement>();

            if (value != null)
            {
                using (var document = value is string json
                           ? JsonDocument.Parse(json)
                           : JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(value, SerializationConfig.DefaultSerializeOptions)))
                {
                    foreach (var property in document.RootElement.Clone().EnumerateObject())
                    {
                        elements.Add(property.Name, property.Value);
                    }
                }
            }

            return elements;
        }

        /// <summary>
        /// Convert an object to the desired type using serialization and deserialization.
        /// </summary>
        /// <param name="value">The object to be converted to desired type.</param>
        /// <typeparam name="T">The type of object to convert to.</typeparam>
        /// <returns>The converted object.</returns>
        public static T ToObject<T>(this object value)
        {
            if (value == null)
            {
                return default;
            }

            return value is string json
                ? JsonSerializer.Deserialize<T>(json, SerializationConfig.DefaultDeserializeOptions)
                : JsonSerializer.Deserialize<T>(
                    JsonSerializer.SerializeToUtf8Bytes(value, SerializationConfig.DefaultSerializeOptions),
                    SerializationConfig.DefaultDeserializeOptions);
        }
    }
}
