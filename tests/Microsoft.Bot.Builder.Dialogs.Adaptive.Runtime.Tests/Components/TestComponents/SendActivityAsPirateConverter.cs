// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Runtime.Tests.Components.TestComponents
{
    internal class SendActivityAsPirateConverter : JsonConverter
    {
        private readonly SourceContext _sourceContext;
        private readonly ResourceExplorer _resourceExplorer;

        public SendActivityAsPirateConverter(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            _sourceContext = sourceContext;
            _resourceExplorer = resourceExplorer;
        }

        public SendActivityAsPirateConverter()
        {
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(SendActivityAsPirate).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                // If we expect an activity but find text, it is a short expression for a message activity.
                return new SendActivityAsPirate(reader.Value as string);
            }

            return new SendActivityAsPirate();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
