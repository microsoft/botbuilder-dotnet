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
    public class BoolExpressionConverter : JsonConverter<BoolExpression>
    {
        public BoolExpressionConverter()
        {
        }
        
        public override bool CanRead => true;

        public override BoolExpression ReadJson(JsonReader reader, Type objectType, BoolExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new BoolExpression((string)reader.Value);
            }
            else
            {
                return new BoolExpression(JToken.Load(reader));
            }
        }

        public override void WriteJson(JsonWriter writer, BoolExpression value, JsonSerializer serializer)
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
