// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// JsonConverter which understands serializes StaticActiviteTemplates as activity object.
    /// </summary>
    public class StaticActivityTemplateConverter : JsonConverter<StaticActivityTemplate>
    {
        public override bool CanRead => false;

        public override bool CanWrite => true; 

        public override StaticActivityTemplate ReadJson(JsonReader reader, Type objectType, StaticActivityTemplate existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, StaticActivityTemplate value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Activity);
        }
    }
}
