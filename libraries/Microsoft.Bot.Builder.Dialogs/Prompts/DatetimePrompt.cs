// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DateTime;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DateTimePrompt : Prompt<IList<DateTimeResolution>>
    {
        public DateTimePrompt(string dialogId, PromptValidator<IList<DateTimeResolution>> validator = null, string defaultLocale = null)
            : base(dialogId, validator)
        {
            DefaultLocale = defaultLocale;
        }

        public string DefaultLocale { get; set; }

        protected override async Task OnPromptAsync(ITurnContext context, IDictionary<string, object> state, PromptOptions options, bool isRetry)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (isRetry && options.RetryPrompt != null)
            {
                await context.SendActivityAsync(options.RetryPrompt).ConfigureAwait(false);
            }
            else if (options.Prompt != null)
            {
                await context.SendActivityAsync(options.Prompt).ConfigureAwait(false);
            }
        }

        protected override async Task<PromptRecognizerResult<IList<DateTimeResolution>>> OnRecognizeAsync(ITurnContext context, IDictionary<string, object> state, PromptOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var result = new PromptRecognizerResult<IList<DateTimeResolution>>();
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var message = context.Activity.AsMessageActivity();
                var culture = context.Activity.Locale ?? DefaultLocale ?? English;
                var results = DateTimeRecognizer.RecognizeDateTime(message.Text, culture);
                if (results.Count > 0)
                {
                    // Return list of resolutions from first match
                    result.Succeeded = true;
                    result.Value = new List<DateTimeResolution>();
                    var values = (List<Dictionary<string, string>>)results[0].Resolution["values"];
                    foreach (var value in values)
                    {
                        result.Value.Add(ReadResolution(value));
                    }
                }
            }

            return result;
        }

        private DateTimeResolution ReadResolution(IDictionary<string, string> resolution)
        {
            var result = new DateTimeResolution();

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
