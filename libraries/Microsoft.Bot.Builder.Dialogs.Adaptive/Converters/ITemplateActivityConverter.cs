// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// JsonConverter to load ITemplate&lt;Activity&gt;.
    /// </summary>
    internal class ITemplateActivityConverter : InterfaceConverter<ITemplate<Activity>>
    {
        internal ITemplateActivityConverter(ResourceExplorer resourceExplorer, SourceContext sourceContext)
            : base(resourceExplorer, sourceContext)
        {
        }

        public override bool CanRead => true;

        // if this is false, don't custom serialize activitytemplate as a string
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(ITemplate<Activity>).IsAssignableFrom(objectType);
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
                return base.ReadJson(reader, objectType, existingValue, serializer);
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

        // ActivityTemplateConverter treats unknown objects as Activity objects, which get wrapped as StaticActivityTemplate instances
        public override object ResolveUnknownObject(JToken jToken)
        {
            var jObject = jToken as JObject;
            if (jObject != null)
            {
                return new StaticActivityTemplate()
                {
                    Activity = jObject.ToObject<Activity>()
                };
            }

            return base.ResolveUnknownObject(jToken);
        }
    }
}
