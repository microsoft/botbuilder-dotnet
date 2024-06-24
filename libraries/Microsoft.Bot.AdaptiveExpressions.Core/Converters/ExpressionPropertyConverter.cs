// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Bot.AdaptiveExpressions.Core.Properties;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    /// <typeparam name="T">The property type to construct.</typeparam>
    public class ExpressionPropertyConverter<T> : JsonConverter<ExpressionProperty<T>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionPropertyConverter{T}"/> class.
        /// </summary>
        public ExpressionPropertyConverter()
        {
        }

        /// <summary>
        /// Reads and converts the JSON type.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override ExpressionProperty<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var typeInfo = options.GetTypeInfo(typeof(T));
            if (reader.TokenType == JsonTokenType.String)
            {
                return new ExpressionProperty<T>(reader.GetString(), typeInfo);
            }
            else
            {
                return new ExpressionProperty<T>(JsonValue.Parse(ref reader), typeInfo);
            }
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, ExpressionProperty<T> value, JsonSerializerOptions options)
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
