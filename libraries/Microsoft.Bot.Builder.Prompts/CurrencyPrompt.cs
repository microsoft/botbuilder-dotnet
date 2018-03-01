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
    /// CurrencyPrompt recognizes currency expressions as float type
    /// </summary>
    public class CurrencyPrompt : NumberWithUnitPrompt
    {
        public CurrencyPrompt(string culture, PromptValidator<NumberWithUnit> validator = null) 
            : base(NumberWithUnitRecognizer.Instance.GetCurrencyModel(culture), validator)
        {
        }

        protected CurrencyPrompt(IModel model, PromptValidator<NumberWithUnit> validator = null) 
            : base(model, validator)
        {
        }
    }
}
