using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    public abstract class BasePrompt<T>
    {
        private readonly PromptValidator<T> _customValidator = null;

        public BasePrompt(PromptValidator<T> validator = null)
        {
            _customValidator = validator;
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
            context.Reply(activity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// implement to recognize the basic type
        /// </summary>
        /// <param name="context"></param>
        /// <returns>null if not recognized</returns>
        public abstract Task<T> Recognize(IBotContext context);


        protected virtual Task<bool> Validate(IBotContext context, T value)
        {
            // Validation passed. Return the validated text.
            if (_customValidator != null)
            {
                return _customValidator(context, value);
            }
            return Task.FromResult(true);
        }

    }
}