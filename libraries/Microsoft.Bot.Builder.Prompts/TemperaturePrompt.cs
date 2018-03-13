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
    /// TemperaturePrompt recognizes temperature expressions 
    /// </summary>
    public class TemperaturePrompt : NumberWithUnitPrompt
    {
        public TemperaturePrompt(string culture, PromptValidator<NumberWithUnit> validator = null) 
            : base(new NumberWithUnitRecognizer(culture).GetTemperatureModel(), validator)
        {
        }

        protected TemperaturePrompt(IModel model, PromptValidator<NumberWithUnit> validator = null) 
            : base(model, validator)
        {
        }
    }
}
