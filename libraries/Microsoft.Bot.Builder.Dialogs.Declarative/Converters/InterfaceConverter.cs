// Copyright (c) Microsoft Corporation. All rights reserved.
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
        private readonly List<IConverterObserver> observers = new List<IConverterObserver>();
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
            var (jToken, range) = SourceScope.ReadTokenRange(reader, sourceContext);
            using (new SourceScope(sourceContext, range))
            {
                if (this.resourceExplorer.IsRef(jToken))
                {
                    // We can't do this asynchronously as the Json.NET interface is synchronous
                    jToken = this.resourceExplorer.ResolveRefAsync(jToken, sourceContext).GetAwaiter().GetResult();
                }

                foreach (var observer in this.observers)
                {
                    if (observer.OnBeforeLoadToken(jToken, out T interceptResult))
                    {
                        return interceptResult;
                    }
                }

                var kind = (string)jToken["$kind"];
              
                if (kind == null)
                {
                    // see if there is jObject resolver
                    var result = ResolveUnknownObject(jToken);
                    if (result != null)
                    {
                        return result;
                    }

                    throw new ArgumentNullException($"$kind was not found: {JsonConvert.SerializeObject(jToken)}");
                }

                // if reference resolution made a source context available for the JToken, then add it to the context stack
                var found = DebugSupport.SourceMap.TryGetValue(jToken, out var rangeResolved);
                using (found ? new SourceScope(sourceContext, rangeResolved) : null)
                {
                    T result = this.resourceExplorer.BuildType<T>(kind, jToken, serializer);

                    // associate the most specific source context information with this item
                    if (sourceContext.CallStack.Count > 0)
                    {
                        range = sourceContext.CallStack.Peek().DeepClone();
                        DebugSupport.SourceMap.Add(result, range);
                    }

                    foreach (var observer in this.observers)
                    {
                        if (observer.OnAfterLoadToken(jToken, result, out T interceptedResult))
                        {
                            return interceptedResult;
                        }
                    }

                    return result;
                }
            }
        }

        public virtual object ResolveUnknownObject(JToken jToken)
        {
            return null;
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
