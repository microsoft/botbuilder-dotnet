// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// JsonConverter which understands serializes StaticActiviteTemplates as activity object.
    /// </summary>
    [SuppressMessage("Performance", "CA1812", Justification = "The class gets instantiated by dependency injection.")]
    internal class StaticActivityTemplateConverter : JsonConverter<StaticActivityTemplate>
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
