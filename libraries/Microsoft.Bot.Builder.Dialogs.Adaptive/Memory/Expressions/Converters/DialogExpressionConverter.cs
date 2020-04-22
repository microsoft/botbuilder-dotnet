// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    public class DialogExpressionConverter : JsonConverter<DialogExpression>
    {
        private readonly InterfaceConverter<Dialog> converter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogExpressionConverter"/> class.
        /// </summary>
        /// <param name="resourceExplorer">resource explorer to use for resolving references.</param>
        /// <param name="sourceContext">SourceContext to build debugger source map.</param>
        public DialogExpressionConverter(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            this.converter = new InterfaceConverter<Dialog>(resourceExplorer, sourceContext);
        }

        public override bool CanRead => true;

        public override DialogExpression ReadJson(JsonReader reader, Type objectType, DialogExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                var id = (string)reader.Value;
                if (id.StartsWith("="))
                {
                    return new DialogExpression(id);
                }

                try
                {
                    return new DialogExpression((Dialog)this.converter.ReadJson(new JsonTextReader(new StringReader($"\"{id}\"")), objectType, existingValue, serializer));
                }
                catch (Exception)
                {
                    return new DialogExpression($"='{id}'");
                }
            }

            return new DialogExpression((Dialog)this.converter.ReadJson(reader, objectType, existingValue, serializer));
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
    }
}
