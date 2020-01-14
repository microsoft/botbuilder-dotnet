// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.AI.LuisV3;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
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
            var json = JsonConvert.SerializeObject(obj.list);
            return new DynamicList()
            {
                Entity = obj.entity,
                List = serializer.Deserialize<List<ListElement>>(new JsonTextReader(new StringReader(json)))
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (DynamicList)value;
            var obj = new JObject(new JProperty("entity", list.Entity));
            serializer.Serialize(writer, obj);
        }
    }
}
