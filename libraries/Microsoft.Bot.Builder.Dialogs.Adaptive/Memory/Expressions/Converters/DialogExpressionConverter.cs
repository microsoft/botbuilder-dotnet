// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Observers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    public class DialogExpressionConverter : JsonConverter<DialogExpression>, IObservableConverter, IObservableJsonConverter
    {
        private readonly Dictionary<string, DialogExpression> cache = new Dictionary<string, DialogExpression>(StringComparer.OrdinalIgnoreCase);
        private readonly InterfaceConverter<Dialog> converter;
        private readonly ResourceExplorer resourceExplorer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogExpressionConverter"/> class.
        /// </summary>
        /// <param name="resourceExplorer">resource explorer to use for resolving references.</param>
        /// <param name="sourceContext">SourceContext to build debugger source map.</param>
        public DialogExpressionConverter(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            this.converter = new InterfaceConverter<Dialog>(resourceExplorer, sourceContext);
            this.resourceExplorer = resourceExplorer;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Newtonsoft.Json.JsonConverter"/> can read JSON.
        /// </summary>
        /// <value>
        /// <c>true</c>.
        /// </value>
        public override bool CanRead => true;

        /// <summary>
        /// Reads the JSON representation of a <see cref="DialogExpression"/> object.
        /// </summary>
        /// <param name="reader">The <see cref="Newtonsoft.Json.JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of <see cref="DialogExpression"/> being read. If there is no existing value then null will be used.</param>
        /// <param name="hasExistingValue">Indicates if existingValue has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The interpreted <see cref="DialogExpression"/> object.</returns>
        public override DialogExpression ReadJson(JsonReader reader, Type objectType, DialogExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);

            DialogExpression result = null;

            if (reader.ValueType == typeof(string))
            {
                var id = (string)reader.Value;
                if (id.StartsWith("=", StringComparison.Ordinal))
                {
                    result = UpdateOrCreateExpression(id);
                }
                else
                {
                    try
                    {
                        using (var jsonTextReader = new JsonTextReader(new StringReader($"\"{id}\"")))
                        {
                            var dialog = (Dialog)converter.ReadJson(jsonTextReader, objectType, existingValue, serializer);
                            result = UpdateOrCreateExpression(id, dialog);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        throw;
                    }
#pragma warning disable CA1031 // Do not catch general exception types (return an empty if an exception happens).
                    catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        result = UpdateOrCreateExpression($"='{id}'");
                    }
                }
            }
            else
            {
                using (var jTokenReader = new JTokenReader(jToken))
                {
                    var dialog = (Dialog)this.converter.ReadJson(jTokenReader, objectType, existingValue, serializer);
                    result = UpdateOrCreateExpression(dialog?.Id, dialog);
                }
            }

            return result;
        }

        /// <summary>
        /// Writes the JSON representation of a <see cref="DialogExpression"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="Newtonsoft.Json.JsonWriter"/> to write to.</param>
        /// <param name="value">The value <see cref="DialogExpression"/>.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, DialogExpression value, JsonSerializer serializer)
        {
            if (value.ExpressionText != null)
            {
                serializer.Serialize(writer, value.ToString());
            }
            else
            {
                serializer.Serialize(writer, value.Value);
            }
        }

        /// <inheritdoc/>
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
        /// Registers an observer to receive notifications on converter events.
        /// </summary>
        /// <param name="observer">The <see cref="IJsonLoadObserver"/> to be registered.</param>
        public void RegisterObserver(IJsonLoadObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            this.converter.RegisterObserver(observer);
        }

        private DialogExpression UpdateOrCreateExpression(string id, Dialog dialog = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Expected non-empty dialog id in expression.", nameof(id));
            }

            if (cache.ContainsKey(id) && !id.StartsWith("=", StringComparison.Ordinal))
            {
                cache[id].SetValue(dialog);
                return cache[id];
            }
            else
            {
                DialogExpression result;
                if (id.StartsWith("=", StringComparison.Ordinal))
                {
                    result = new DialogExpression(id);
                }
                else
                {
                    result = new DialogExpression((Dialog)dialog);
                    cache.Add(id, result);
                }
                
                return result;
            }
        }
    }
}
