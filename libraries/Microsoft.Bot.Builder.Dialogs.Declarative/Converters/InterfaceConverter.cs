// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative.Debugger;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    public class InterfaceConverter<T> : JsonConverter where T : class
    {
        private readonly IRefResolver refResolver;
        private readonly Source.IRegistry registry;
        private readonly Stack<string> paths;

        public InterfaceConverter(IRefResolver refResolver, Source.IRegistry registry, Stack<string> paths)
        {
            this.refResolver = refResolver ?? throw new ArgumentNullException(nameof(refResolver));
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            this.paths = paths ?? throw new ArgumentNullException(nameof(paths));
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = Source.Point.Read(reader, JToken.Load, out var start, out var after);

            if (refResolver.IsRef(jsonObject))
            {
                // We can't do this asynchronously as the Json.NET interface is synchronous
                jsonObject = refResolver.ResolveAsync(jsonObject).GetAwaiter().GetResult();
            }

            //jsonObject["id"] = jsonObject["id"] ?? jsonObject["$id"];

            var typeName = jsonObject["$type"]?.ToString();
            if (typeName == null)
            {
                throw new ArgumentNullException(JsonConvert.SerializeObject(jsonObject));
            }

            var found = this.registry.TryGetValue(jsonObject, out var range);

            if (found)
            {
                paths.Push(range.Path);
            }

            T result = TypeFactory.Build<T>(typeName, jsonObject, serializer);

            range = new Source.Range() { Path = paths.Peek(), Start = start, After = after };
            this.registry.Add(result, range);

            if (found)
            {
                paths.Pop();
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
