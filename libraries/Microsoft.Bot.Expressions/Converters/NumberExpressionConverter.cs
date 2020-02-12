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
    public class NumberExpressionConverter : JsonConverter<NumberExpression>
    {
        public NumberExpressionConverter()
        {
        }

        public override bool CanRead => true;

        public override NumberExpression ReadJson(JsonReader reader, Type objectType, NumberExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new NumberExpression((string)reader.Value);
            }
            else
            {
                return new NumberExpression(JToken.Load(reader));
            }
        }

        public override void WriteJson(JsonWriter writer, NumberExpression value, JsonSerializer serializer)
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
