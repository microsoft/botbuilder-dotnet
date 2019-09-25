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
        private readonly ISourceMap sourceMap;

        public DialogSchemaConverter(IRefResolver refResolver, ISourceMap sourceMap)
        {
            this.refResolver = refResolver ?? throw new ArgumentNullException(nameof(refResolver));
            this.sourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
            => typeof(DialogSchema) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = SourcePoint.ReadObjectWithSourcePoints(reader, JToken.Load, out var start, out var after);

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
