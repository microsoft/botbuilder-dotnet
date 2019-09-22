// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object
    /// </summary>
    /// <typeparam name="T">The property type to construct which is IExpressionProperty</typeparam>
    public class ExpressionPropertyConverter<T> : JsonConverter
        where T : IExpressionProperty, new()
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                var prop = new T();
                prop.SetValue((string)reader.Value);
                return prop;
            }
            else
            {
                var prop = new T();
                prop.SetValue(JToken.Load(reader));
                return prop;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
