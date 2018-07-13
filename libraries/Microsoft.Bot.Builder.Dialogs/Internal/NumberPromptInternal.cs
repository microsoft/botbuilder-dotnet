// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// NumberPrompt recognizes floats or ints
    /// </summary>
    internal class NumberPromptInternal<T> : BasePromptInternal<NumberResult<T>>
    {
        private IModel _model;

        public NumberPromptInternal(string culture, PromptValidator<NumberResult<T>> validator = null)
            : base(validator)
        {
            _model = new NumberRecognizer(culture).GetNumberModel(culture);
        }

        protected NumberPromptInternal(IModel model, PromptValidator<NumberResult<T>> validator = null)
            : base(validator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Used to validate the incoming text, expected on context.Activity, is
        /// valid according to the rules defined in the validation steps.
        /// </summary>
        public override async Task<NumberResult<T>> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            NumberResult<T> numberResult = new NumberResult<T>();

            var message = context.Activity as MessageActivity;
            var results = _model.Parse(message.Text);
            if (results.Any())
            {
                var result = results.First();
                if (typeof(T) == typeof(float))
                {
                    if (float.TryParse(result.Resolution["value"].ToString(), out float value))
                    {
                        numberResult.Status = PromptStatus.Recognized;
                        numberResult.Value = (T)(object)value;
                        numberResult.Text = result.Text;
                        await Validate(context, numberResult);
                    }
                }
                else if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(result.Resolution["value"].ToString(), out int value))
                    {
                        numberResult.Status = PromptStatus.Recognized;
                        numberResult.Value = (T)(object)value;
                        numberResult.Text = result.Text;
                        await Validate(context, numberResult);
                    }
                }
                else if (typeof(T) == typeof(long))
                {
                    if (long.TryParse(result.Resolution["value"].ToString(), out long value))
                    {
                        numberResult.Status = PromptStatus.Recognized;
                        numberResult.Value = (T)(object)value;
                        numberResult.Text = result.Text;
                        await Validate(context, numberResult);
                    }
                }
                else if (typeof(T) == typeof(double))
                {
                    if (double.TryParse(result.Resolution["value"].ToString(), out double value))
                    {
                        numberResult.Status = PromptStatus.Recognized;
                        numberResult.Value = (T)(object)value;
                        numberResult.Text = result.Text;
                        await Validate(context, numberResult);
                    }
                }
                else if (typeof(T) == typeof(decimal))
                {
                    if (decimal.TryParse(result.Resolution["value"].ToString(), out decimal value))
                    {
                        numberResult.Status = PromptStatus.Recognized;
                        numberResult.Value = (T)(object)value;
                        numberResult.Text = result.Text;
                        await Validate(context, numberResult);
                    }
                }
                else
                {
                    throw new NotSupportedException($"type argument T of type 'typeof(T)' is not supported");
                }
            }
            return numberResult;
        }
    }
}
