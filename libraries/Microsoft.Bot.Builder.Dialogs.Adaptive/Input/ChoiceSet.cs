using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Defines Choices Property as either collection of Choices, array of strings or string which is an expression to one of thoses
    /// </summary>
    public class ChoiceSet : ExpressionProperty<List<Choice>>
    {
        public ChoiceSet()
        {
        }

        public ChoiceSet(string expression)
            : base(expression)
        {
        }

        public ChoiceSet(List<Choice> choices)
            : base(choices)
        {
        }

        public ChoiceSet(object choices)
            : base(choices)
        {
        }

        protected override List<Choice> ConvertObject(object result)
        {
            // support string[] => choice[]
            if (result is IEnumerable<string> strings)
            {
                return strings.Select(s => new Choice(s)).ToList();
            }

            // support JArray to => hoice
            if (result is JArray array)
            {
                var choices = new List<Choice>();
                if (array.HasValues)
                {
                    foreach (var element in array)
                    {
                        if (element is JValue jval)
                        {
                            choices.Add(new Choice(element.ToString()));
                        }
                        else if (element is JObject jobj)
                        {
                            choices.Add(jobj.ToObject<Choice>());
                        }
                    }
                }

                return choices;
            }

            return JArray.FromObject(result).ToObject<List<Choice>>();
        }
    }
}
