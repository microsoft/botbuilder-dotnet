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
        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceSet"/> class.
        /// </summary>
        public ChoiceSet()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceSet"/> class.
        /// </summary>
        /// <param name="choices">Choice values.</param>
        public ChoiceSet(IEnumerable<Choice> choices)
            : base(choices)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceSet"/> class.
        /// </summary>
        /// <param name="obj">Choice values.</param>
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

        /// <summary>
        /// Converts a bool into a <see cref="ChoiceSet"/>.
        /// </summary>
        /// <param name="value">Bool expression.</param>
        public static implicit operator ChoiceSet(bool value) => new ChoiceSet(value);

        /// <summary>
        /// Converts a string into a <see cref="ChoiceSet"/>.
        /// </summary>
        /// <param name="value">String expression.</param>
        public static implicit operator ChoiceSet(string value) => new ChoiceSet(value);

        /// <summary>
        /// Converts a <see cref="JToken"/> into a <see cref="ChoiceSet"/>.
        /// </summary>
        /// <param name="value"><see cref="JToken"/> expression.</param>
        public static implicit operator ChoiceSet(JToken value) => new ChoiceSet(value);
    }
}
