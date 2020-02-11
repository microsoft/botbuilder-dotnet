// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// JsonConverter to load ITemplate&lt;Activity&gt;.
    /// </summary>
    public class ActivityTemplateConverter : JsonConverter
    {
        public override bool CanRead => true;

        // if this is false, don't custom serialize activitytemplate as a string
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(ITemplate<Activity>) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                // inline template Example: "Hello {name}"
                string readerValue = reader.Value.ToString();
                return new ActivityTemplate((string)readerValue);
            }
            else
            {
                JObject obj = JObject.Load(reader);
                string kind = (string)obj["$kind"] ?? (string)obj["$type"]; 
                if (kind == "Microsoft.ActivityTemplate")
                {
                    return obj.ToObject<ActivityTemplate>();
                }

                var activity = obj.ToObject<Activity>();
                return new StaticActivityTemplate((Activity)activity);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // save template as string
            if (value is ActivityTemplate activityTemplate)
            {
                serializer.Serialize(writer, activityTemplate.Template);
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }
    }
}
