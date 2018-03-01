using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using Microsoft.Recognizers.Text.NumberWithUnit;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{

    /// <summary>
    /// AgePrompt recognizes age expressions like "95 years"
    /// </summary>
    public class AgePrompt : NumberWithUnitPrompt
    {
        public AgePrompt(string culture, PromptValidator<NumberWithUnit> validator = null)
            : base(NumberWithUnitRecognizer.Instance.GetAgeModel(culture), validator)
        {
        }

        protected AgePrompt(IModel model, PromptValidator<NumberWithUnit> validator = null)
            : base(model, validator)
        {
        }
    }
}
