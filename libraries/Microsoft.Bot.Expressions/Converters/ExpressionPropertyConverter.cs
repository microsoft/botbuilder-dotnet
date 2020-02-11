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
    /// <typeparam name="T">The property type to construct.</typeparam>
    public class ExpressionPropertyConverter<T> : JsonConverter<ExpressionProperty<T>>
    {
        public override bool CanRead => true;

        public override ExpressionProperty<T> ReadJson(JsonReader reader, Type objectType, ExpressionProperty<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new ExpressionProperty<T>((string)reader.Value);
            }
            else
            {
                return new ExpressionProperty<T>(JToken.Load(reader));
            }
        }

        public override void WriteJson(JsonWriter writer, ExpressionProperty<T> value, JsonSerializer serializer)
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
