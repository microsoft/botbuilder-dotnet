// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// CurrencyPrompt recognizes currency expressions as float type.
    /// </summary>
    internal class DateTimePromptInternal : BasePromptInternal<DateTimeResult>
    {
        private IModel _model;

        public DateTimePromptInternal(string culture, PromptValidator<DateTimeResult> validator = null)
            : base(validator)
        {
            _model = new DateTimeRecognizer(culture).GetDateTimeModel();
        }

        protected DateTimePromptInternal(IModel model, PromptValidator<DateTimeResult> validator = null)
            : base(validator)
        {
            _model = model;
        }

        /// <summary>
        /// Used to validate the incoming text, expected on context.Request, is
        /// valid according to the rules defined in the validation steps.
        /// </summary>
        /// <param name="context">Context for the current turn of the conversation with the user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DateTimeResult> RecognizeAsync(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var message = context.Activity.AsMessageActivity();
                var results = _model.Parse(message.Text);
                if (results.Any())
                {
                    var values = (List<Dictionary<string, string>>)results[0].Resolution["values"];

                    var dateTimeResult = new DateTimeResult
                    {
                        Status = PromptStatus.Recognized,
                        Text = message.Text,
                    };

                    foreach (var value in values)
                    {
                        dateTimeResult.Resolution.Add(ReadResolution(value));
                    }

                    await ValidateAsync(context, dateTimeResult).ConfigureAwait(false);
                    return dateTimeResult;
                }
            }

            return new DateTimeResult();
        }

        private DateTimeResult.DateTimeResolution ReadResolution(Dictionary<string, string> resolution)
        {
            var result = new DateTimeResult.DateTimeResolution();

            if (resolution.TryGetValue("timex", out var timex))
            {
                result.Timex = timex;
            }

            if (resolution.TryGetValue("value", out var value))
            {
                result.Value = value;
            }

            if (resolution.TryGetValue("start", out var start))
            {
                result.Start = start;
            }

            if (resolution.TryGetValue("end", out var end))
            {
                result.End = end;
            }

            return result;
        }
    }
}
