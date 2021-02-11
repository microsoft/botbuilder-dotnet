// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    /// <typeparam name="T">The type of the items of the array.</typeparam>
    public class ArrayExpressionConverter<T> : JsonConverter<ArrayExpression<T>>
    {
        /// <summary>
        /// Gets a value indicating whether this Converter can read JSON.
        /// </summary>
        /// <value>true if this Converter can read JSON; otherwise, false.</value>
        public override bool CanRead => true;

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The Newtonsoft.Json.JsonReader to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="hasExistingValue">A boolean value indicating whether there is an existing value of object to be read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>An ArrayExpression instance.</returns>
        public override ArrayExpression<T> ReadJson(JsonReader reader, Type objectType, ArrayExpression<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new ArrayExpression<T>((string)reader.Value);
            }
            else
            {
                // NOTE: This does not use the serializer because even we could deserialize here
                // expression evaluation has no idea about converters.
                return new ArrayExpression<T>(JToken.Load(reader));
            }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The Newtonsoft.Json.JsonWriter to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, ArrayExpression<T> value, JsonSerializer serializer)
        {
            if (value.ExpressionText != null)
            {
                serializer.Serialize(writer, value.ToString());
            }
            else
            {
                serializer.Serialize(writer, value.Value);
            }
        }
    }
}
