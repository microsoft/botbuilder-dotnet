// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using Microsoft.Bot.Builder.AI.LuisV3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis
{
    public class DynamicListConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(DynamicList) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            dynamic obj = JToken.Load(reader);
            return new DynamicList()
            {
                Entity = obj.entity,
                List = obj.list.ToObject<List<ListElement>>()
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (DynamicList)value;
            writer.WriteStartObject();
            writer.WritePropertyName("entity");
            writer.WriteValue(list.Entity);
            writer.WritePropertyName("list");
            serializer.Serialize(writer, list.List);
            writer.WriteEndObject();
        }
    }
}
