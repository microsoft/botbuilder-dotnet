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
    /// DimensionPrompt recognizes dimension expressions like "4 feet" or "6 miles"
    /// </summary>
    public class DimensionPrompt : NumberWithUnitPrompt
    {
        public DimensionPrompt(string culture, PromptValidator<NumberWithUnit> validator = null) 
            : base(NumberWithUnitRecognizer.Instance.GetDimensionModel(culture), validator)
        {
        }

        protected DimensionPrompt(IModel model, PromptValidator<NumberWithUnit> validator = null)
            : base(model, validator)
        {
        }
    }
}
