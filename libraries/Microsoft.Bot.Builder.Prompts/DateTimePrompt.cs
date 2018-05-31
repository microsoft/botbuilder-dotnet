// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    /// <summary>
    /// The result of a DateTime prompt
    /// Note there might be 1, 2 or 4 resolutions depending on the particular scenario.
    /// For example:
    /// - a specific date and time like "5th December 2018 at 9am" results in a single resolution
    /// - a date with some ambiguity like "4th October" results in a single TIMEX but still 2 example values and so 2 resolutions
    /// - a date and time with ambiguity like Octerber 4 4 Oclock" results in two TIMXE and 4 example values so 4 resolutions
    /// </summary>
    public class DateTimeResult : PromptResult
    {
        public DateTimeResult()
        {
            Resolution = new List<DateTimeResolution>();
        }

        /// <summary>
        /// The input text recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public string Text
        {
            get { return GetProperty<string>(nameof(Text)); }
            set { this[nameof(Text)] = value; }
        }

        /// <summary>
        /// The various resolutions for the recognized value; or and empty list.
        /// </summary>
        public List<DateTimeResolution> Resolution
        {
            get { return GetProperty<List<DateTimeResolution>>(nameof(Resolution)); }
            private set { this[nameof(Resolution)] = value; }
        }

        public class DateTimeResolution
        {
            public string Value { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public string Timex { get; set; }
        }
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
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var message = context.Activity.AsMessageActivity();
                var results = _model.Parse(message.Text);
                if (results.Any())
                {
                    var result = results.First();
                    if (result.Resolution.Any())
                    {
                        var dateTimeResult = new DateTimeResult
                        {
                            Status = PromptStatus.Recognized,
                            Text = result.Text
                        };

                        foreach (var resolution in result.Resolution)
                        {
                            var values = (List<Dictionary<string, string>>)resolution.Value;
                            if (values.Any())
                            {
                                dateTimeResult.Resolution.Add(ReadResolution(values.First()));
                                await Validate(context, dateTimeResult);
                                return dateTimeResult;
                            }
                        }
                    }
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