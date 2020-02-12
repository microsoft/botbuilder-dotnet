// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Expressions.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Expressions.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    public class IntExpressionConverter : JsonConverter<IntExpression>
    {
        public IntExpressionConverter()
        {
        }

        public override bool CanRead => true;

        public override IntExpression ReadJson(JsonReader reader, Type objectType, IntExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new IntExpression((string)reader.Value);
            }
            else
            {
                return new IntExpression(JToken.Load(reader));
            }
        }

        public override void WriteJson(JsonWriter writer, IntExpression value, JsonSerializer serializer)
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
