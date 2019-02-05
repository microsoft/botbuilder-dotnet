// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Composition;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Parsers;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Types;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Converters
{
    public class ActivityTemplateConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(ActivityTemplate) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            ActivityTemplate template;
            
            if (reader.ValueType == typeof(string))
            {
                string readerValue = reader.Value.ToString();

                var args = TemplateExpressionParser.Parse(readerValue);

                // Direct string in text
                if (args == null || args.Count == 0)
                {
                    template = new ActivityTemplate(readerValue);
                }
                // Template
                else
                {
                    template = new ActivityTemplate()
                    {
                        Template = args.First()
                    };
                }
                // TODO: hybrid model, i.e. inline template: "Hello {name}"
            }
            else
            {
                template = JToken.Load(reader).ToObject<ActivityTemplate>();
            }

            // TODO: Wire language generator into configuration pipeline
            template.LanguageGenerator = new LangugageGenerator("en-us.lg");

            return template;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
