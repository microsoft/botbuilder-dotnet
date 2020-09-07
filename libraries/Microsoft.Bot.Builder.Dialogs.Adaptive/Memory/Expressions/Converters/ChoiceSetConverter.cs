// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Converter for ChoiceSet - allows string or array initializers.
    /// </summary>
    public class ChoiceSetConverter : JsonConverter<ChoiceSet>
    {
        /// <summary>
        /// Reads the JSON representation of a <see cref="ChoiceSet"/> object.
        /// </summary>
        /// <param name="reader">The <see cref="Newtonsoft.Json.JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of <see cref="ChoiceSet"/> being read. If there is no existing value then null will be used.</param>
        /// <param name="hasExistingValue">Indicates if existingValue has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The interpreted <see cref="ChoiceSet"/> object.</returns>
        public override ChoiceSet ReadJson(JsonReader reader, Type objectType, ChoiceSet existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new ChoiceSet((string)reader.Value);
            }
            else
            {
                return new ChoiceSet(JArray.Load(reader));
            }
        }

        /// <summary>
        /// Writes the JSON representation of a <see cref="ChoiceSet"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="Newtonsoft.Json.JsonWriter"/> to write to.</param>
        /// <param name="choiceSet">The <see cref="ChoiceSet"/>.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, ChoiceSet choiceSet, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var choice in choiceSet)
            {
                JObject.FromObject(choice).WriteTo(writer);
            }

            writer.WriteEndArray();
        }
    }
}
