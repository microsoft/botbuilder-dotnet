// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    public class InterfaceConverter<T> : JsonConverter where T : class
    {
        private readonly IRefResolver refResolver;

        public InterfaceConverter(IRefResolver refResolver)
        {
            this.refResolver = refResolver ?? throw new ArgumentNullException(nameof(refResolver));
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JToken.Load(reader);

            if (refResolver.IsRef(jsonObject))
            {
                jsonObject = refResolver.Resolve(jsonObject);
            }

            //jsonObject["id"] = jsonObject["id"] ?? jsonObject["$id"];

            var typeName = jsonObject["$type"]?.ToString();
            T result = Factory.Build<T>(typeName, jsonObject, serializer);

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
