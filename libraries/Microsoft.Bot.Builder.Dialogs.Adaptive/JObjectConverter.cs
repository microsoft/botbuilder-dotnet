// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    public class JObjectConverter : JsonConverter
    {
        private readonly IRefResolver refResolver;

        public JObjectConverter(IRefResolver refResolver)
        {
            this.refResolver = refResolver ?? throw new ArgumentNullException(nameof(refResolver));
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
            => typeof(JObject) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = SourcePoint.ReadObjectWithSourcePoints(reader, JToken.Load, out var start, out var after);

            if (refResolver.IsRef(jsonObject))
            {
                // We can't do this asynchronously as the Json.NET interface is synchronous
                jsonObject = refResolver.ResolveAsync(jsonObject).GetAwaiter().GetResult();
            }

            return jsonObject as JObject;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
