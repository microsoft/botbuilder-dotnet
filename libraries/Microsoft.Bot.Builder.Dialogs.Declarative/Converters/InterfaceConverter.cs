// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Observers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    /// <summary>
    /// Converts an object to and from JSON.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    public class InterfaceConverter<T> : JsonConverter, IObservableConverter, IObservableJsonConverter
        where T : class
    {
        private readonly ResourceExplorer resourceExplorer;
        private readonly List<IJsonLoadObserver> observers = new List<IJsonLoadObserver>();
        private readonly SourceContext sourceContext;
        private readonly Dictionary<string, T> cachedRefDialogs = new Dictionary<string, T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceConverter{T}"/> class.
        /// </summary>
        /// <param name="resourceExplorer">A ResourceExplorer object to initialize the current instance.</param>
        /// <param name="sourceContext">A SourceContext object to initialize the current instance.</param>
        public InterfaceConverter(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            this.resourceExplorer = resourceExplorer ?? throw new ArgumentNullException(nameof(InterfaceConverter<T>.resourceExplorer));
            this.sourceContext = sourceContext ?? throw new ArgumentNullException(nameof(sourceContext));
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="InterfaceConverter{T}"/> can read JSON.
        /// </summary>
        /// /// <value><c>true</c> if this <see cref="InterfaceConverter{T}"/> can read JSON; otherwise, <c>false</c>.</value>
        public override bool CanRead => true;

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///     <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var (jToken, range) = SourceScope.ReadTokenRange(reader, sourceContext);

            using (new SourceScope(sourceContext, range))
            {
                string refDialogName = null;
                if (resourceExplorer.IsRef(jToken))
                {
                    refDialogName = jToken.Value<string>();

                    // We can't do this asynchronously as the Json.NET interface is synchronous
                    jToken = resourceExplorer.ResolveRefAsync(jToken, sourceContext).GetAwaiter().GetResult();
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
                using (var newScope = found ? new SourceScope(sourceContext, rangeResolved) : null)
                {
                    var passTwo = false;
                    foreach (var observer in this.observers)
                    {
                        if (observer is CycleDetectionObserver cycDetectObserver && cycDetectObserver.CycleDetectionPass == CycleDetectionPasses.PassTwo)
                        {
                            passTwo = true;
                        }

                        if (observer.OnBeforeLoadToken(sourceContext, rangeResolved ?? range, jToken, out T interceptResult))
                        {
                            return interceptResult;
                        }
                    }

                    var tokenToBuild = TryAssignId(jToken, sourceContext);

                    T result;
                    if (passTwo && refDialogName != null && cachedRefDialogs.ContainsKey(refDialogName))
                    {
                        result = cachedRefDialogs[refDialogName];
                    }
                    else
                    {
                        result = this.resourceExplorer.BuildType<T>(kind, tokenToBuild, serializer);
                        if (passTwo && refDialogName != null)
                        {
                            cachedRefDialogs[refDialogName] = result;
                        }
                    }

                    // Associate the most specific source context information with this item
                    if (sourceContext.CallStack.Count > 0)
                    {
                        range = sourceContext.CallStack.Peek().DeepClone();
                        if (!DebugSupport.SourceMap.TryGetValue(result, out var _))
                        {
                            DebugSupport.SourceMap.Add(result, range);
                        }
                    }

                    foreach (var observer in this.observers)
                    {
                        if (observer.OnAfterLoadToken(sourceContext, range, jToken, result, out T interceptedResult))
                        {
                            return interceptedResult;
                        }
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Performs an action on an unknown object.
        /// </summary>
        /// <param name="jToken">The unknown object to resolve.</param>
        /// <returns>An object value.</returns>
        public virtual object ResolveUnknownObject(JToken jToken)
        {
            return null;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        /// <summary>
        /// Registers a <see cref="IConverterObserver"/> to receive notifications on converter events.
        /// </summary>
        /// <param name="observer">The observer to be registered.</param>
        [Obsolete("Deprecated in favor of IJsonLoadObserver registration.")]
        public void RegisterObserver(IConverterObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            // Create a wrapper for the deprecated type IConverterObserver.
            // This supports backward compartibity.
            RegisterObserver(new JsonLoadObserverWrapper(observer));
        }

        /// <summary>
        /// Registers a <see cref="IJsonLoadObserver"/> to receive notifications on converter events.
        /// </summary>
        /// <param name="observer">The observer to be registered.</param>
        public void RegisterObserver(IJsonLoadObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            this.observers.Add(observer);
        }

        private static JToken TryAssignId(JToken jToken, SourceContext sourceContext)
        {
            // If our JToken does not have an id, try to get an id from the resource explorer
            // in a best-effort manner.
            if (jToken is JObject jObj && !jObj.ContainsKey("id"))
            {
                // Check if we have an id registered for this token
                if (sourceContext is ResourceSourceContext rsc && rsc.DefaultIdMap.ContainsKey(jToken))
                {
                    // Just assign. Avoid cloning JTokens since Json.NET does not clone line info
                    // Tracking item on Json.NET: https://github.com/JamesNK/Newtonsoft.Json/issues/2410
                    jToken["id"] = rsc.DefaultIdMap[jToken];
                }
            }

            return jToken;
        }
    }
}
