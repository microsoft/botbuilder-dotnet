using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using Microsoft.Recognizers.Text.NumberWithUnit;
using Microsoft.Recognizers.Text.Sequence;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    /// <summary>
    /// PhoneNumberPrompt recognizes phone numbers
    /// </summary>
    public class PhoneNumberPrompt : ValuePrompt
    {        
        public PhoneNumberPrompt(string culture, PromptValidator<TextResult> validator = null) :
            base(new SequenceRecognizer(culture).GetPhoneNumberModel(), validator)
        {
            // ToDo: The creation of the new Recognizer is expensive given all of the
            // Regex compilation in there. If we need to optimize this, we can add a static 
            // concurrent dictionary here based on culture. The model.parse() method called 
            // in the base class is thread safe. 
        }

        protected PhoneNumberPrompt(IModel model, PromptValidator<TextResult> validator = null) :
            base(model, validator)
        {
        }

    }
}
