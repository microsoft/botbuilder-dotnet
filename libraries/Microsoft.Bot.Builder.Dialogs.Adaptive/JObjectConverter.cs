// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    public class JObjectConverter : JsonConverter
    {
        private readonly ResourceExplorer resourceExplorer;

        public JObjectConverter(ResourceExplorer resourceExplorer)
        {
            this.resourceExplorer = resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer));
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
            => typeof(JObject) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = SourcePoint.ReadObjectWithSourcePoints(reader, JToken.Load, out var start, out var after);

            if (resourceExplorer.IsRef(jsonObject))
            {
                // We can't do this asynchronously as the Json.NET interface is synchronous
                jsonObject = resourceExplorer.ResolveRefAsync(jsonObject).GetAwaiter().GetResult();
            }

            return jsonObject as JObject;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
