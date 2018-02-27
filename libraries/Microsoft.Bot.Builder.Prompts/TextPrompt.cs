using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
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
    public class TextPrompt 
    {
        private readonly PromptValidator<string, string> _customValidator = null;

        /// <summary>
        /// Creates a new instance of a TextPrompt. 
        /// </summary>
        public TextPrompt()
        {
        }

        /// <summary>
        /// Creates a new instance of a TextPrompt allowing a custom validator
        /// to be specified. The custom validator will ONLY be called if the
        /// Validate method on the class first passes. 
        /// </summary>
        public TextPrompt(PromptValidator<string, string> validator)
        {
            _customValidator = validator ?? throw new ArgumentNullException(nameof(validator)); 
        }

        /// <summary>
        /// Creates a new Message, and queues it for sending to the user. 
        /// </summary>
        public Task Prompt(IBotContext context, string text, string speak = null)
        {
            IMessageActivity activity = MessageFactory.Text(text, speak);
            activity.InputHint = InputHints.ExpectingInput;
            return Prompt(context, activity);
        }

        /// <summary>
        /// Creates a new Message Activity, and queues it for sending to the user. 
        /// </summary>
        public Task Prompt(IBotContext context, IMessageActivity activity)
        {
            context.Responses.Add(activity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Used to validate the incoming text, expected on context.Request, is
        /// valid according to the rules defined in the validation steps. 
        /// </summary>        
        /// <returns>Tuple with the following values:
        /// Passed: [true/false]. Indicates if the validation passed or failed.
        /// Value: IF the validation passed, the validated string is retured. 
        /// If the validation failed, the value of this is not defined.
        /// </returns>
        /// <remarks>
        /// The Tuple is returned to allow both value and reference types to be 
        /// validated, allowig this pattern to be used across numeric recognizers
        /// DateTime recoginizers, and other types. Because value types are 
        /// non-nullable, the common pattern of returning null breaks down in 
        /// those scenarios.
        /// </remarks>
        public Task<(bool Passed, string Value)> Recognize(IBotContext context)
        {
            BotAssert.ContextNotNull(context); 
            BotAssert.ActivityNotNull(context.Request);
            if (context.Request.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            IMessageActivity message = context.Request.AsMessageActivity();

            if (_customValidator == null)
            {
                // No additional validator. Just call the virtual method. 
                return this.Validate(context, message);
            }
            else
            {
                return _customValidator(context, message.Text);
            }            
        }

        protected virtual async Task<(bool Passed, string Value)> Validate(IBotContext context, IMessageActivity activity)
        {
            if (string.IsNullOrWhiteSpace(activity.Text))
            {
                // Validation failed. 
                return (Passed: false, Value: activity.Text);
            }
            else
            {
                // Validation passed. Return the validated text.
                return (Passed: true, Value: activity.Text);
            }

        }
    }
}
