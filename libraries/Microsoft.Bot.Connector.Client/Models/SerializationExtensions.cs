// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Bot.Connector.Client.Models
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
            if (value == null)
            {
                return new Dictionary<string, JsonElement>();
            }

            if (value is Dictionary<string, JsonElement> result)
            {
                return result;
            }

            var elements = new Dictionary<string, JsonElement>();

            if (value is string json)
            {
                if (!string.IsNullOrWhiteSpace(json))
                {
                    using (var document = JsonDocument.Parse(json))
                    {
                        foreach (var property in document.RootElement.Clone().EnumerateObject())
                        {
                            elements.Add(property.Name, property.Value);
                        }
                    }
                }
            }
            else
            {
                using (var document = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(value, SerializationConfig.DefaultSerializeOptions)))
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

            if (value is T result)
            {
                return result;
            }

            return value is string json
                ? json.Deserialize<T>()
                : JsonSerializer.Deserialize<T>(
                    JsonSerializer.SerializeToUtf8Bytes(value, SerializationConfig.DefaultSerializeOptions),
                    SerializationConfig.DefaultDeserializeOptions);
        }

        /// <summary>
        /// Deserialize a JSON string to the desired type.
        /// </summary>
        /// <param name="value">The string to be deserialized.</param>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value, SerializationConfig.DefaultDeserializeOptions);
        }
    }
}
