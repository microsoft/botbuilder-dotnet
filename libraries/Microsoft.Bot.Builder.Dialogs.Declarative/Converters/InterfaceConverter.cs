// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    public class InterfaceConverter<T> : JsonConverter 
        where T : class
    {
        private readonly IRefResolver refResolver;
        private readonly ISourceMap sourceMap;
        private readonly Stack<string> paths;

        public InterfaceConverter(IRefResolver refResolver, ISourceMap sourceMap, Stack<string> paths)
        {
            this.refResolver = refResolver ?? throw new ArgumentNullException(nameof(refResolver));
            this.sourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));
            this.paths = paths ?? throw new ArgumentNullException(nameof(paths));
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = SourcePoint.ReadObjectWithSourcePoints(reader, JToken.Load, out SourcePoint startPoint, out SourcePoint endPoint);

            if (refResolver.IsRef(jsonObject))
            {
                // We can't do this asynchronously as the Json.NET interface is synchronous
                jsonObject = refResolver.ResolveAsync(jsonObject).GetAwaiter().GetResult();
            }

            // jsonObject["id"] = jsonObject["id"] ?? jsonObject["$id"];

            var typeName = jsonObject["$type"]?.ToString();
            if (typeName == null)
            {
                throw new ArgumentNullException(JsonConvert.SerializeObject(jsonObject));
            }

            // if IdRefResolver made a path available for the JToken, then add it to the path stack
            // this maintains the stack of paths used as the source of json data
            var found = this.sourceMap.TryGetValue(jsonObject, out var range);
            if (found)
            {
                paths.Push(range.Path);
            }

            T result = TypeFactory.Build<T>(typeName, jsonObject, serializer);

            // DeclarativeTypeLoader.LoadAsync only adds FileResource to the paths stack
            if (paths.Count > 0)
            {
                // combine the "path for the most recent JToken from IdRefResolver" or the "top root path"
                // with the line information for this particular json fragment and add it to the sourceMap
                range = new SourceRange() { Path = paths.Peek(), StartPoint = startPoint, EndPoint = endPoint };
                this.sourceMap.Add(result, range);
            }

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
