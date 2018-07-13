// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// CurrencyPrompt recognizes currency expressions as float type
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
        public override async Task<DateTimeResult> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var message = context.Activity as MessageActivity;
                var results = _model.Parse(message.Text);
                if (results.Any())
                {
                    var values = (List<Dictionary<string, string>>)results[0].Resolution["values"];

                    var dateTimeResult = new DateTimeResult
                    {
                        Status = PromptStatus.Recognized,
                        Text = message.Text
                    };

                    foreach (var value in values)
                    {
                        dateTimeResult.Resolution.Add(ReadResolution(value));
                    }

                    await Validate(context, dateTimeResult);
                    return dateTimeResult;
                }
            }
            return new DateTimeResult();
        }

        private DateTimeResult.DateTimeResolution ReadResolution(Dictionary<string, string> resolution)
        {
            var result = new DateTimeResult.DateTimeResolution();

            string timex;
            if (resolution.TryGetValue("timex", out timex))
            {
                result.Timex = timex;
            }
            string value;
            if (resolution.TryGetValue("value", out value))
            {
                result.Value = value;
            }
            string start;
            if (resolution.TryGetValue("start", out start))
            {
                result.Start = start;
            }
            string end;
            if (resolution.TryGetValue("end", out end))
            {
                result.End = end;
            }

            return result;
        }
    }
}
