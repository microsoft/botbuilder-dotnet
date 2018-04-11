using System;
using Microsoft.Recognizers.Text.Choice;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{

    /// <summary>
    /// ConfirmPrompt recognizes confrimation expressions as bool 
    /// </summary>
    public class ConfirmPrompt : ChoicePrompt<bool>
    {
        public ConfirmPrompt(string culture, PromptValidator<ChoiceResult<bool>> validator = null)
            : base(new ChoiceRecognizer(culture).GetBooleanModel(), validator)
        {
            // ToDo: The creation of the new Recognizer is expensive given all of the
            // Regex compilation in there. If we need to optimize this, we can add a static 
            // concurrent dictionary here based on culture. The model.parse() method called 
            // in the base class is thread safe.
        }

        public ConfirmPrompt(IModel model, PromptValidator<ChoiceResult<bool>> validator = null)
            : base(model, validator)
        {
        }
    }
}