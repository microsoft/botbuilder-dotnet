﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AdaptiveExpressions.Core.Properties;

namespace Microsoft.AdaptiveExpressions.Core.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    public class StringExpressionConverter : JsonConverter<StringExpression>
    {
        /// <summary>
        /// Reads and converts the JSON type.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override StringExpression Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new StringExpression(reader.GetString());
            }
            else
            {
                return new StringExpression(JsonValue.Parse(ref reader));
            }
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, StringExpression value, JsonSerializerOptions options)
        {
            if (value.ExpressionText != null)
            {
                writer.WriteStringValue(value.ToString());
            }
            else
            {
                writer.WriteStringValue(value.Value);
            }
        }
    }
}
