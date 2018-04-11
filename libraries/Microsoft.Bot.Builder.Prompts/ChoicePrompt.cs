using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    public class ChoiceResult<T> : PromptResult
    {
        public T Value { get; set; }
        public string Text { get; set; }
        public float Confidence { get; set; }
    }

    public class ChoicePrompt<T> : BasePrompt<ChoiceResult<T>>
    {
        private IModel _model;
        
        public ChoicePrompt(IModel model, PromptValidator<ChoiceResult<T>> validator = null)
            : base(validator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public override async Task<ChoiceResult<T>> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            ChoiceResult<T> choiceResult = new ChoiceResult<T>();

            IMessageActivity message = context.Activity.AsMessageActivity();
            var results = _model.Parse(message.Text);
            if (results.Any())
            {
                var result = results.First();
                if (typeof(T) == typeof(bool))
                {
                    if (bool.TryParse(result.Resolution["value"].ToString(), out bool value))
                    {
                        choiceResult.Status = PromptStatus.Recognized;
                        choiceResult.Value = (T)(object)value;
                        choiceResult.Text = result.Text;
                        choiceResult.Confidence = GetConfidence(result);
                        await Validate(context, choiceResult);
                    }
                }
                else
                {
                    choiceResult.Status = PromptStatus.Recognized;
                    choiceResult.Value = (T)result.Resolution["value"];
                    choiceResult.Text = result.Text;
                    choiceResult.Confidence = GetConfidence(result);
                    await Validate(context, choiceResult);
                }
            }
            return choiceResult;
        }

        private float GetConfidence(ModelResult result)
        {
            if (float.TryParse(result.Resolution["score"].ToString(), out float confidence))
            {
                return confidence;
            }
            return 0f;
        }
    }
}
