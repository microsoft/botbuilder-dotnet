using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    public class TextResult : PromptResult
    {
        public TextResult() { }

        public string Value { get; set; }

        public string Text { get; set; }
    }


    /// <summary>
    /// Text Prompt provides a simple mechanism to send text to a user
    /// and validate a response. The default validator passes on any 
    /// non-whitespace string. That behavior is easily changed by deriving
    /// from this class and authoring custom validation behavior. 
    /// 
    /// For simple validation changes, a PromptValidator may be passed in to the 
    /// constructor. If the standard validation passes, the custom PromptValidator
    /// will be called. 
    /// </summary>
    public class TextPrompt : BasePrompt<TextResult>
    {

        /// <summary>
        /// Creates a new instance of a TextPrompt allowing a custom validator
        /// to be specified. The custom validator will ONLY be called if the
        /// Validate method on the class first passes. 
        /// </summary>
        public TextPrompt(PromptValidator<TextResult> validator = null) 
            :base(validator)
        {
        }

        /// <summary>
        /// Used to validate the incoming text, expected on context.Activity, is
        /// valid according to the rules defined in the validation steps. 
        /// </summary>        
        public override async Task<TextResult> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            IMessageActivity message = context.Activity.AsMessageActivity();
            TextResult textResult = new TextResult();
            if (message.Text != null)
            {
                textResult.Status = PromptStatus.Recognized;
                textResult.Value = message.Text;
                textResult.Text = message.Text;
                await Validate(context, textResult);
            }
            return textResult;
        }

    }
}
