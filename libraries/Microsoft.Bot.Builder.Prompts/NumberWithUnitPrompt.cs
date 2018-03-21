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
    public class NumberWithUnit : PromptResult
    {
        public NumberWithUnit()
        {
            Value = float.NaN;
        }

        public string Unit { get; set; }

        public float Value { get; set; }

        public string Text { get; set; }
    }

    /// <summary>
    /// CurrencyPrompt recognizes currency expressions as float type
    /// </summary>
    public class NumberWithUnitPrompt : BasePrompt<NumberWithUnit>
    {
        private IModel _model;


        protected NumberWithUnitPrompt(IModel model, PromptValidator<NumberWithUnit> validator = null)
            : base(validator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Used to validate the incoming text, expected on context.Activity, is
        /// valid according to the rules defined in the validation steps. 
        /// </summary>        
        public override async Task<NumberWithUnit> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            IMessageActivity message = context.Activity.AsMessageActivity();
            NumberWithUnit value = new NumberWithUnit();
            var results = _model.Parse(message.Text);
            if (results.Any())
            {
                var result = results.First();
                value.Unit = (string)result.Resolution["unit"];
                if (float.TryParse(result.Resolution["value"]?.ToString() ?? String.Empty, out float val))
                {
                    value.Status = PromptStatus.Recognized;
                    value.Text = result.Text;
                    value.Value = val;
                    await Validate(context, value);
                }
            }
            return value;
        }

    }
}
