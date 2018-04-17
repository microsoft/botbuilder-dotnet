using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Prompts
{
    public class Choice
    {
        public string Value { get; set; }
        public IEnumerable<string> Synonyms { get; set; }
        public CardAction Action { get; set; }
    }

    public class ChoicePrompt : ChoicePrompt<string>
    {
        public IList<Choice> Choices { get; private set; }

        public ChoicePrompt(string culture, IList<Choice> choices, PromptValidatorEx.PromptValidator<ChoiceResult<string>> validator = null, bool allowPartialMatch = false, int maxDistance = 2)
            : base(culture, choices.ToDictionary(), validator, allowPartialMatch, maxDistance)
        {
            Choices = choices;
        }

        public ChoicePrompt(string culture, IDictionary<IEnumerable<string>, string> choices, PromptValidatorEx.PromptValidator<ChoiceResult<string>> validator = null, bool allowPartialMatch = false, int maxDistance = 2)
            : base(culture, choices, validator, allowPartialMatch, maxDistance)
        {
            Choices = new List<Choice>();
            foreach (var item in choices)
            {
                Choices.Add(new Choice() {
                    Value = item.Value,
                    Synonyms = item.Key,
                });
            }
        }
    }
}
