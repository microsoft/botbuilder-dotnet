using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    public class TextPrompt 
    {
        private readonly PromptValidator<string, string> _customValidator = null;  

        public TextPrompt()
        {
        }        

        public TextPrompt(PromptValidator<string, string> validator)
        {
            _customValidator = validator ?? throw new ArgumentNullException(nameof(validator)); 
        }

        public Task Prompt(IBotContext context, string text, string speak = null)
        {
            IMessageActivity activity = MessageFactory.Text(text, speak);
            activity.InputHint = InputHints.ExpectingInput;
            return Prompt(context, activity);
        }

        public Task Prompt(IBotContext context, IMessageActivity activity)
        {
            context.Responses.Add(activity);
            return Task.CompletedTask;
        }

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
                return (Passed: false, Value: string.Empty);
            }
            else
            {
                return (Passed: true, Value: activity.Text);
            }

        }
    }
}
