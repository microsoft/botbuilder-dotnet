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
    /// CurrencyPrompt recognizes currency expressions as float type
    /// </summary>
    public class ValuePrompt : BasePrompt<TextResult>
    {
        private IModel _model;


        protected ValuePrompt(IModel model, PromptValidator<TextResult> validator = null) : base(validator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
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

            TextResult textResult = new TextResult();
            IMessageActivity message = context.Activity.AsMessageActivity();
            var results = _model.Parse(message.Text);
            if (results.Any())
            {
                var result = results.First();
                textResult.Status = PromptStatus.Recognized;
                textResult.Text = result.Text;
                textResult.Value = (string)result.Resolution["value"];
                await Validate(context, textResult);
            }
            return textResult;
        }
    }
}
