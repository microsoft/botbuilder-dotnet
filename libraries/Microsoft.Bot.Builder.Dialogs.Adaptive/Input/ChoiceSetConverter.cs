using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public class ChoiceSetConverter : JsonConverter<ChoiceSet>
    {
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
