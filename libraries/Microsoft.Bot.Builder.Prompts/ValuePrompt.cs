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
    public class ValueResult
    {
        public ValueResult() { }

        public string Value { get; set; }

        public string Text { get; set; }
    }

    /// <summary>
    /// CurrencyPrompt recognizes currency expressions as float type
    /// </summary>
    public class ValuePrompt : BasePrompt<ValueResult>
    {
        private IModel _model;


        protected ValuePrompt(IModel model, PromptValidator<ValueResult> validator = null) : base(validator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Used to validate the incoming text, expected on context.Request, is
        /// valid according to the rules defined in the validation steps. 
        /// </summary>        
        public override async Task<ValueResult> Recognize(IBotContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Request);
            if (context.Request.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            IMessageActivity message = context.Request.AsMessageActivity();
            var results = _model.Parse(message.Text);
            if (results.Any())
            {
                var result = results.First();
                ValueResult value = new ValueResult()
                {
                    Text = result.Text,
                    Value = (string)result.Resolution["value"]
                };

                if (await Validate(context, value))
                    return value;
            }
            return null;
        }
    }
}
