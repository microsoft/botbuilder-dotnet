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
    /// <typeparam name="T">The enum type to construct.</typeparam>
    public class EnumExpressionConverter<T> : JsonConverter<EnumExpression<T>>
        where T : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpressionConverter{T}"/> class.
        /// </summary>
        public EnumExpressionConverter()
        {
        }

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
        /// <returns>An EnumExpression instance.</returns>
        public override EnumExpression<T> ReadJson(JsonReader reader, Type objectType, EnumExpression<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new EnumExpression<T>((string)reader.Value);
            }
            else
            {
                return new EnumExpression<T>(JToken.Load(reader));
            }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The Newtonsoft.Json.JsonWriter to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, EnumExpression<T> value, JsonSerializer serializer)
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
