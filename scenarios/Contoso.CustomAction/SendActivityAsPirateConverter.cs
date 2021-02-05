﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Contoso.CustomAction
{
    internal class SendActivityAsPirateConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(SendActivityAsPirateConverter).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                // If we expect an activity but find text, it is a short expression for a message activity.
                return new Activity()
                {
                    Type = ActivityTypes.Message,
                    Text = (string)reader.Value
                };
            }

            return JToken.Load(reader).ToObject<Activity>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
