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

        public override bool CanRead => true;

        public override DialogExpression ReadJson(JsonReader reader, Type objectType, DialogExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);

            DialogExpression result = null;

            if (reader.ValueType == typeof(string))
            {
                var id = (string)reader.Value;
                if (id.StartsWith("=", StringComparison.Ordinal))
                {
                    result = new DialogExpression(id);
                }
                else
                {
                    try
                    {
                        using (var jsonTextReader = new JsonTextReader(new StringReader($"\"{id}\"")))
                        {
                            result = new DialogExpression((Dialog)converter.ReadJson(jsonTextReader, objectType, existingValue, serializer));
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types (return an empty if an exception happens).
                    catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        result = new DialogExpression($"='{id}'");
                    }
                }
            }
            else
            {
                using (var jTokenReader = new JTokenReader(jToken))
                {
                    result = new DialogExpression((Dialog)this.converter.ReadJson(jTokenReader, objectType, existingValue, serializer));
                }
            }

            return result;
        }

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

        public void RegisterObserver(IJsonLoadObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            this.converter.RegisterObserver(observer);
        }
    }
}
