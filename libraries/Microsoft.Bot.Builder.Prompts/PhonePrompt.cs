using System;
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
            base(SequenceRecognizer.Instance.GetPhoneNumberModel(culture), validator)
        {
        }

        protected PhoneNumberPrompt(IModel model, PromptValidator<TextResult> validator = null) :
            base(model, validator)
        {
        }

    }
}
