using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Defines ChoiceSet collection.
    /// </summary>
    [JsonConverter(typeof(ChoiceSetConverter))]
    public class ChoiceSet : List<Choice>
    {
        public ChoiceSet()
        {
        }

        public ChoiceSet(IEnumerable<Choice> choices)
            : base(choices)
        {
        }

        public ChoiceSet(object obj)
        {
            // support string[] => choice[]
            if (obj is IEnumerable<string> strings)
            {
                foreach (var str in strings)
                {
                    this.Add(new Choice(str));
                }
            }

            // support JArray to => choice
            if (obj is JArray array)
            {
                if (array.HasValues)
                {
                    foreach (var element in array)
                    {
                        if (element is JValue jval)
                        {
                            this.Add(new Choice(element.ToString()));
                        }
                        else if (element is JObject jobj)
                        {
                            this.Add(jobj.ToObject<Choice>());
                        }
                    }
                }
            }
        }

        public static implicit operator ChoiceSet(bool value) => new ChoiceSet(value);

        public static implicit operator ChoiceSet(string value) => new ChoiceSet(value);

        public static implicit operator ChoiceSet(JToken value) => new ChoiceSet(value);
    }
}
