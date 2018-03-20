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
            : base(new NumberWithUnitRecognizer(culture).GetDimensionModel(), validator)
        {
            // ToDo: The creation of the new Recognizer is expensive given all of the
            // Regex compilation in there. If we need to optimize this, we can add a static 
            // concurrent dictionary here based on culture. The model.parse() method called 
            // in the base class is thread safe.
        }

        protected DimensionPrompt(IModel model, PromptValidator<NumberWithUnit> validator = null)
            : base(model, validator)
        {
        }
    }
}
