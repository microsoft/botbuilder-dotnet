// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Form.Converters
{
    public class DialogSchemaConverter : JsonConverter
    {
        private readonly IRefResolver refResolver;
        private readonly Source.IRegistry registry;

        public DialogSchemaConverter(IRefResolver refResolver, Source.IRegistry registry)
        {
            this.refResolver = refResolver ?? throw new ArgumentNullException(nameof(refResolver));
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
            => typeof(DialogSchema) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = Source.Point.Read(reader, JToken.Load, out var start, out var after);

            if (refResolver.IsRef(jsonObject))
            {
                // We can't do this asynchronously as the Json.NET interface is synchronous
                jsonObject = refResolver.ResolveAsync(jsonObject).GetAwaiter().GetResult();
            }

            return new DialogSchema(jsonObject as JObject);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
