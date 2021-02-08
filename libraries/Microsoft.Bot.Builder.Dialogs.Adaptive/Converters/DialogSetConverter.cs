// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    public class DialogSetConverter : JsonConverter<DialogSet>
    {
        private readonly ResourceExplorer resourceExplorer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogSetConverter"/> class.
        /// </summary>
        /// <param name="resourceExplorer">resource explorer to use for resolving references.</param>
        public DialogSetConverter(ResourceExplorer resourceExplorer)
        {
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
        /// Reads the JSON representation of a <see cref="DialogSet"/> object.
        /// </summary>
        /// <param name="reader">The <see cref="Newtonsoft.Json.JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of <see cref="DialogSet"/> being read. If there is no existing value then null will be used.</param>
        /// <param name="hasExistingValue">Indicates if existingValue has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The interpreted <see cref="DialogSet"/> object.</returns>
        public override DialogSet ReadJson(JsonReader reader, Type objectType, DialogSet existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);
            var result = new DialogSet();
            if (jToken is JArray arr)
            {
                foreach (var element in arr)
                {
                    if (element.Type == JTokenType.String)
                    {
                        string dialogName = element.ToObject<string>();
                        var resourceId = resourceExplorer.GetResource($"{dialogName}.dialog");
                        var dialog = resourceExplorer.LoadType<Dialog>(resourceId);
                        result.Add(dialog);
                    }
                    else if (element.Type == JTokenType.Object)
                    {
                        var dialog = element.ToObject<AdaptiveDialog>(serializer);
                        result.Add(dialog);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Writes the JSON representation of a <see cref="DialogSet"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="Newtonsoft.Json.JsonWriter"/> to write to.</param>
        /// <param name="value">The value <see cref="DialogExpression"/>.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, DialogSet value, JsonSerializer serializer)
        {
            var dialogs = value.GetDialogs();
            var dialogsName = new List<string>();
            foreach (var dialog in dialogs)
            {
                dialogsName.Add(dialog.Id);
            }

            serializer.Serialize(writer, dialogsName);
        }
    }
}
