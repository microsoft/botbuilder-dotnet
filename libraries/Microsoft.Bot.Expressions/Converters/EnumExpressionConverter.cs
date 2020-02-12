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
    /// <typeparam name="T">The enum type to construct.</typeparam>
    public class EnumExpressionConverter<T> : JsonConverter<EnumExpression<T>>
        where T : struct
    {
        public EnumExpressionConverter()
        {
        }

        public override bool CanRead => true;

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
