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
    public class DialogExpressionConverter : JsonConverter<DialogExpression>, IObservableConverter
    {
        private readonly InterfaceConverter<Dialog> converter;
        private readonly ResourceExplorer resourceExplorer;
        private readonly List<IConverterObserver> observers = new List<IConverterObserver>();

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

            foreach (var observer in observers)
            {
                if (observer.OnBeforeLoadToken(jToken, out DialogExpression interceptResult))
                {
                    return interceptResult;
                }
            }

            DialogExpression result = null;

            if (reader.ValueType == typeof(string))
            {
                var id = (string)reader.Value;
                if (id.StartsWith("="))
                {
                    result = new DialogExpression(id);
                }
                else
                {
                    try
                    {
                        result = new DialogExpression((Dialog)this.converter.ReadJson(new JsonTextReader(new StringReader($"\"{id}\"")), objectType, existingValue, serializer));
                    }
                    catch (Exception)
                    {
                        result = new DialogExpression($"='{id}'");
                    }
                }
            }
            else
            {
                result = new DialogExpression((Dialog)this.converter.ReadJson(new JTokenReader(jToken), objectType, existingValue, serializer));
            }

            foreach (var observer in observers)
            {
                if (observer.OnAfterLoadToken(jToken, result, out DialogExpression interceptedResult))
                {
                    interceptedResult.SetValue(result.Value);
                    result = interceptedResult;
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
        public void RegisterObserver(IConverterObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            this.observers.Add(observer);
            this.converter.RegisterObserver(observer);
        }
    }
}
