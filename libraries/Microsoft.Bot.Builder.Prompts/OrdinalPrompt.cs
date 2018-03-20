using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{

    /// <summary>
    /// OrdinalPrompt recognizes pharses like First, 2nd, third, etc.
    /// </summary>
    public class OrdinalPrompt : NumberPrompt<int>
    {

        public OrdinalPrompt(string culture, PromptValidator<NumberResult<int>> validator = null) 
            : base(new NumberRecognizer(culture).GetOrdinalModel(), validator)
        {
        }

        protected OrdinalPrompt(IModel model, PromptValidator<NumberResult<int>> validator = null) 
            : base(model, validator)
        {
        }
        
    }
}
