using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    public class ChoiceResult<T> : PromptResult
    {
        /// <summary>
        /// The value recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// The input text recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The accuracy with which the value matched the specified portion of the text.
        /// </summary>
        public float Confidence { get; set; }
    }

    /// <summary>
    /// ChoicePrompt recognizes a choice from a list of possible values
    /// </summary>
    public class ChoicePrompt<T> : BasePrompt<ChoiceResult<T>>
    {
        private IModel _model;
        private string _culture;
        private ICollection<T> _values;

        /// <summary>
        /// Creates a <see cref="ChoicePrompt{T}"/> object.
        /// </summary>
        /// <param name="culture">The culture used in the internal recognizer.</param>
        /// <param name="values">A dictionary with a list of possible values and the recognized value the each list.</param>
        /// <param name="validator">The input validator for the prompt object.</param>
        /// <param name="allowPartialMatch">If true, then only some of the words in a value need to exist to be considered a match. The default value is "false".</param>
        /// <param name="maxDistance">Maximum words allowed between two matched words in the utterance.</param>
        /// <remarks><paramref name="validator"/> is called only if the
        /// <see cref="Recognize(ITurnContext)"/> method recognizes a value. 
        /// </remarks>
        public ChoicePrompt(string culture, IDictionary<IEnumerable<string>, T> values, PromptValidator<ChoiceResult<T>> validator = null, bool allowPartialMatch = false, int maxDistance = 2)
            : base(validator)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            _culture = culture ?? throw new ArgumentNullException(nameof(culture));
            _values = values.Values;
            _model = new ChoiceModelBuilder<T>(values)
                .WithAllowPartialMatch(allowPartialMatch)
                .WithMaxDistance(maxDistance)
                .WithCulture(culture)
                .Build();
        }

        /// <summary>
        /// Creates a <see cref="ChoicePrompt{T}"/> object.
        /// </summary>
        /// <param name="culture">The culture used in the internal recognizer.</param>
        /// <param name="values">A dictionary with a regex to match and the recognized value for each regex.</param>
        /// <param name="validator">The input validator for the prompt object.</param>
        /// <param name="allowPartialMatch">If true, then only some of the words in a value need to exist to be considered a match. The default value is "false".</param>
        /// <param name="maxDistance">Maximum words allowed between two matched words in the utterance.</param>
        /// <remarks><paramref name="validator"/> is called only if the
        /// <see cref="Recognize(ITurnContext)"/> method recognizes a value. 
        /// </remarks>
        public ChoicePrompt(string culture, IDictionary<Regex, T> values, PromptValidator<ChoiceResult<T>> validator = null, bool allowPartialMatch = false, int maxDistance = 2)
            : base(validator)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            _culture = culture ?? throw new ArgumentNullException(nameof(culture));
            _values = values.Values;
            _model = new ChoiceModelBuilder<T>(values)
                .WithAllowPartialMatch(allowPartialMatch)
                .WithMaxDistance(maxDistance)
                .WithCulture(culture)
                .Build();
        }
        
        /// <summary>
        /// Creates a <see cref="ChoicePrompt{T}"/> object.
        /// </summary>
        /// <param name="model">The model used in the internal recognizer.</param>
        /// <param name="validator">The input validator for the prompt object.</param>
        /// <remarks><paramref name="validator"/> is called only if the
        /// <see cref="Recognize(ITurnContext)"/> method recognizes a value. 
        /// </remarks>
        public ChoicePrompt(IModel model, PromptValidator<ChoiceResult<T>> validator = null)
            : base(validator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Recognizes and validates the user input.
        /// </summary>
        /// <param name="context">The context for the current turn.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this when you expect that the incoming activity for this
        /// turn contains the user input to recognize.
        /// If recognition succeeds, the <see cref="ChoiceResult{T}.Value"/> property of the 
        /// result contains the value recognized, and the <see cref="ChoiceResult{T}.Confidence"/>
        /// property contains the accuaracywith which the value matched the specified portion of
        /// the activity. A value of 1.0 would indicate a perfect match.
        /// <para>If recognition fails, returns a <see cref="ChoiceResult"/> with
        /// its <see cref="PromptStatus"/> set to <see cref="PromptStatus.NotRecognized"/> and
        /// its <see cref="ChoiceResult{T}.Value"/> set to <c>null</c>.</para>
        /// </remarks>
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
            else if (_values != null)
            {
                if (RecognizeOrdinalIndex(message.Text, out choiceResult))
                {
                    await Validate(context, choiceResult);
                }
                else if (RecognizeCardinalIndex(message.Text, out choiceResult))
                {
                    await Validate(context, choiceResult);
                }
            }
            return choiceResult;
        }

        private bool RecognizeOrdinalIndex(string text, out ChoiceResult<T> choiceResult)
        {
            var results = NumberRecognizer.RecognizeOrdinal(text, _culture);
            if (results.Any())
            {
                var result = results.First();
                if (int.TryParse(result.Resolution["value"].ToString(), out int index))
                {
                    if (TryGetValue(index, out T value))
                    {
                        choiceResult = new ChoiceResult<T>
                        {
                            Status = PromptStatus.Recognized,
                            Value = value,
                            Text = result.Text,
                            Confidence = GetConfidence(result)
                        };
                        return true;
                    }
                }
            }
            choiceResult = new ChoiceResult<T>();
            return false;
        }

        private bool RecognizeCardinalIndex(string text, out ChoiceResult<T> choiceResult)
        {
            var results = NumberRecognizer.RecognizeNumber(text, _culture);
            if (results.Any())
            {
                var result = results.First();
                if (int.TryParse(result.Resolution["value"].ToString(), out int index))
                {
                    if (TryGetValue(index, out T value))
                    {
                        choiceResult = new ChoiceResult<T>
                        {
                            Status = PromptStatus.Recognized,
                            Value = value,
                            Text = result.Text,
                            Confidence = GetConfidence(result)
                        };
                        return true;
                    }
                }
            }
            choiceResult = new ChoiceResult<T>();
            return false;
        }

        private float GetConfidence(ModelResult result)
        {
            if (!result.Resolution.ContainsKey("score"))
            {
                return 1f;
            }

            if (float.TryParse(result.Resolution["score"].ToString(), out float confidence))
            {
                return confidence;
            }

            return 0f;
        }

        private bool TryGetValue(int index, out T result)
        {
            if (index > 0 && index <= _values.Count)
            {
                result = _values.ElementAt(index - 1);
                return true;
            }
            else if (_values.Count + index > 0)
            {
                result = _values.ElementAt(_values.Count + index);
                return true;
            }
            result = default(T);
            return false;
        }
    }
}
