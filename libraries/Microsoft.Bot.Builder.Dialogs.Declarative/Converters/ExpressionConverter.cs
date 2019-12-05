// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    /// <summary>
    /// "string" => Expression object converter.
    /// </summary>
    public class ExpressionConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Expression);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new ExpressionEngine().Parse((string)reader.Value);
            }

            throw new JsonSerializationException("Expected string expression.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            value = ((Expression)value).ToString();
            serializer.Serialize(writer, value);
        }
    }
}
