// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Data.SqlTypes;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    /// <typeparam name="T">The property type to construct which is IExpressionProperty.</typeparam>
    public class LongExpressionConverter : JsonConverter<LongExpression>
    {
        public LongExpressionConverter()
        {
        }

        public override bool CanRead => true;

        public override LongExpression ReadJson(JsonReader reader, Type objectType, LongExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new LongExpression((string)reader.Value);
            }
            else
            {
                return new LongExpression(JToken.Load(reader));
            }
        }

        public override void WriteJson(JsonWriter writer, LongExpression value, JsonSerializer serializer)
        {
            if (value.Expression != null)
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
