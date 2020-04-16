// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    public class InterfaceConverter<T> : JsonConverter
        where T : class
    {
        private readonly ResourceExplorer resourceExplorer;
        private readonly SourceContext sourceContext;

        public InterfaceConverter(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            this.resourceExplorer = resourceExplorer ?? throw new ArgumentNullException(nameof(InterfaceConverter<T>.resourceExplorer));
            this.sourceContext = sourceContext ?? throw new ArgumentNullException(nameof(sourceContext));
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var (jsonObject, range) = SourceScope.ReadTokenRange(reader, sourceContext);
            using (new SourceScope(sourceContext, range))
            {
                if (this.resourceExplorer.IsRef(jsonObject))
                {
                    // We can't do this asynchronously as the Json.NET interface is synchronous
                    jsonObject = this.resourceExplorer.ResolveRefAsync(jsonObject, sourceContext).GetAwaiter().GetResult();
                }

                var kind = (string)jsonObject["$kind"];
                if (kind == null)
                {
                    throw new ArgumentNullException($"$kind was not found: {JsonConvert.SerializeObject(jsonObject)}");
                }

                // if IdRefResolver made a source context available for the JToken, then add it to the context stack
                var found = DebugSupport.SourceMap.TryGetValue(jsonObject, out var rangeResolved);
                using (found ? new SourceScope(sourceContext, rangeResolved) : null)
                {
                    T result = this.resourceExplorer.BuildType<T>(kind, jsonObject, serializer);

                    // associate the most specific source context information with this item
                    if (sourceContext.CallStack.Count > 0)
                    {
                        range = sourceContext.CallStack.Peek().DeepClone();
                        DebugSupport.SourceMap.Add(result, range);
                    }

                    return result;
                }
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
