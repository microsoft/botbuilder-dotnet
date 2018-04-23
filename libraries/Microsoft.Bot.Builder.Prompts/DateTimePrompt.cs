// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    public class DateTimeResult : PromptResult
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Timex { get; set; }
    }

    /// <summary>
    /// CurrencyPrompt recognizes currency expressions as float type
    /// </summary>
    public class DateTimePrompt : BasePrompt<DateTimeResult>
    {
        private IModel _model;

        public DateTimePrompt(string culture, PromptValidator<DateTimeResult> validator = null)
            : base(validator)
        {
            _model = new DateTimeRecognizer(culture).GetDateTimeModel();
        }

        protected DateTimePrompt(IModel model, PromptValidator<DateTimeResult> validator = null)
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
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            var message = context.Activity.AsMessageActivity();
            var results = _model.Parse(message.Text);
            if (results.Any())
            {
                var result = results.First();
                if (result.Resolution.Any())
                {
                    var values = (List<Dictionary<string, string>>)result.Resolution.First().Value;
                    if (values.Any())
                    {
                        var dateTimeResult = new DateTimeResult
                        {
                            Status = PromptStatus.Recognized,
                            Text = result.Text
                        };
                        ReadResolution(dateTimeResult, values.First());
                        await Validate(context, dateTimeResult);
                        return dateTimeResult;
                    }
                }
            }
            return new DateTimeResult();
        }

        private void ReadResolution(DateTimeResult result, Dictionary<string, string> resolution)
        {
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
        }
    }
}
