// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Bot.AdaptiveExpressions.Core.Properties;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Converters
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
        /// Reads and converts the JSON type.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override EnumExpression<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var typeInfo = options.GetTypeInfo(typeof(T));
            if (reader.TokenType == JsonTokenType.String)
            {
                return new EnumExpression<T>(reader.GetString(), typeInfo);
            }
            else
            {
                return new EnumExpression<T>(JsonValue.Parse(ref reader), typeInfo);
            }
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, EnumExpression<T> value, JsonSerializerOptions options)
        {
            if (value.ExpressionText != null)
            {
                writer.WriteStringValue(value.ToString());
            }
            else
            {
                FunctionUtils.SerializeValueToWriter(writer, value.Value, value.ValueJsonTypeInfo, options);
            }
        }
    }
}
