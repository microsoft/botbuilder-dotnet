using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    public class PromptStatus
    {
        /// <summary>
        /// The data type was not recognized at all
        /// </summary>
        public const string NotRecognized = "NotRecognized";

        /// <summary>
        /// Data type was recognized and validated
        /// </summary>
        public const string Recognized = "Recognized";

        /// <summary>
        /// The validation failed because too small
        /// </summary>
        public const string TooSmall = "TooSmall";

        /// <summary>
        /// The validation failed because too big
        /// </summary>
        public const string TooBig = "TooBig";

        /// <summary>
        /// The validation failed because it was out of range
        /// </summary>
        public const string OutOfRange = "OutOfRange";
    }

    public class PromptResult
    {
        public PromptResult()
        {
            Status = PromptStatus.NotRecognized;
        }

        public string Status { get; set; }

        public bool Succeeded() { return Status == PromptStatus.Recognized; }
    }


    public abstract class BasePrompt<T>
        where T : PromptResult
    {
        private readonly PromptValidator<T> _customValidator = null;

        public BasePrompt(PromptValidator<T> validator = null)
        {
            _customValidator = validator;
        }

        /// <summary>
        /// Creates a new Message, and queues it for sending to the user. 
        /// </summary>
        public Task Prompt(ITurnContext context, string text, string speak = null)
        {
            IMessageActivity ma = Activity.CreateMessageActivity();
            ma.Text = !string.IsNullOrWhiteSpace(text) ? text : null;
            ma.Speak = !string.IsNullOrWhiteSpace(speak) ? speak : null;            
            ma.InputHint = InputHints.ExpectingInput;
            return Prompt(context, ma);
        }

        /// <summary>
        /// Creates a new Message Activity, and queues it for sending to the user. 
        /// </summary>
        public async Task Prompt(ITurnContext context, IMessageActivity activity)
        {            
            await context.SendActivity(activity);            
        }

        /// <summary>
        /// implement to recognize the basic type
        /// </summary>
        /// <param name="context"></param>
        /// <returns>null if not recognized</returns>
        public abstract Task<T> Recognize(ITurnContext context);

        protected virtual Task Validate(ITurnContext context, T value)
        {
            // Validation passed. Return the validated text.
            if (_customValidator != null)
            {
                return _customValidator(context, value);
            }
            value.Status = PromptStatus.Recognized;
            return Task.CompletedTask;
        }

    }
}