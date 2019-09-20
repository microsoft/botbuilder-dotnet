// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Converters
{
    /// <summary>
    /// Converter which allows loads json to Choices(expression) or Choices(object)
    /// </summary>
    /// <typeparam name="T">type of property</typeparam>
    public class ChoiceSetConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(ChoiceSet) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new ChoiceSet((string)reader.Value);
            }
            else
            {
                return new ChoiceSet((object)JToken.Load(reader));
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
