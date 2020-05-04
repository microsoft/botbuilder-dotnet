﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Observers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    public class InterfaceConverter<T> : JsonConverter, IObservableConverter
        where T : class
    {
        private readonly ResourceExplorer resourceExplorer;
        private readonly Stack<SourceRange> context;
        private readonly List<IConverterObserver> observers = new List<IConverterObserver>();

        public InterfaceConverter(ResourceExplorer resourceExplorer, Stack<SourceRange> context)
        {
            this.resourceExplorer = resourceExplorer ?? throw new ArgumentNullException(nameof(InterfaceConverter<T>.resourceExplorer));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var (jsonObject, range) = SourceContext.ReadTokenRange(reader, context);

            using (new SourceContext(context, range))
            {
                if (this.resourceExplorer.IsRef(jsonObject))
                {
                    // We can't do this asynchronously as the Json.NET interface is synchronous
                    jsonObject = this.resourceExplorer.ResolveRefAsync(jsonObject, context).GetAwaiter().GetResult();
                }

                foreach (var observer in this.observers)
                {
                    if (observer.OnBeforeLoadToken(jsonObject, out T interceptResult))
                    {
                        return interceptResult;
                    }
                }

                var kind = (string)jsonObject["$kind"];
                if (kind == null)
                {
                    throw new ArgumentNullException($"$kind was not found: {JsonConvert.SerializeObject(jsonObject)}");
                }

                // if reference resolution made a source context available for the JToken, then add it to the context stack
                var found = DebugSupport.SourceMap.TryGetValue(jsonObject, out var rangeResolved);
                using (found ? new SourceContext(context, rangeResolved) : null)
                {
                    T result = this.resourceExplorer.BuildType<T>(kind, jsonObject, serializer);

                    // associate the most specific source context information with this item
                    if (context.Count > 0)
                    {
                        range = context.Peek().DeepClone();
                        DebugSupport.SourceMap.Add(result, range);
                    }

                    foreach (var observer in this.observers)
                    {
                        if (observer.OnAfterLoadToken(jsonObject, result, out T interceptedResult))
                        {
                            return interceptedResult;
                        }
                    }

                    return result;
                }
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        /// <inheritdoc/>
        public void RegisterObserver(IConverterObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            this.observers.Add(observer);
        }
    }
}
