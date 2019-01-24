// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Prompts;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DateTime;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DateTimePromptOptions : PromptOptions
    {
        public DateTime? MinValue { get; set; }

        public DateTime? MaxValue { get; set; }
    }

    public class DateTimePrompt : Prompt<IList<DateTimeResolution>, DateTimePromptOptions>, IRangePromptOptions<DateTime>
    {
        public DateTimePrompt(string dialogId = null, PromptValidator<IList<DateTimeResolution>> validator = null, string defaultLocale = null)
            : base(dialogId, validator)
        {
            DefaultLocale = defaultLocale;
            MinValue = DateTime.MinValue;
            MaxValue = DateTime.MaxValue;
        }

        public string DefaultLocale { get; set; }

        public DateTime MinValue { get; set; }

        public DateTime MaxValue { get; set; }

        public Activity TooSmallResponse { get; set; }

        public Activity TooLargeResponse { get; set; }

        protected override async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, DateTimePromptOptions options, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.MinValue == null)
            {
                options.MinValue = this.MinValue;
            }

            if (options.MaxValue == null)
            {
                options.MaxValue = this.MaxValue;
            }

            if (_validator == null)
            {
                _validator = new PromptValidator<IList<DateTimeResolution>>(async (promptContext, cancel) =>
                {
                    if (!promptContext.Recognized.Succeeded)
                    {
                        if (this.NoMatchResponse != null)
                        {
                            await promptContext.Context.SendActivityAsync(this.NoMatchResponse).ConfigureAwait(false);
                        }

                        await promptContext.Context.SendActivityAsync(options.RetryPrompt ?? this.RetryPrompt ?? options.Prompt ?? this.InitialPrompt).ConfigureAwait(false);
                        return false;
                    }

                    var result = promptContext.Recognized.Value.FirstOrDefault();
                    if (DateTime.TryParse(result.Value, out DateTime value))
                    {

                        if (value < options.MinValue.Value)
                        {
                            if (this.TooSmallResponse != null)
                            {
                                await promptContext.Context.SendActivityAsync(this.TooSmallResponse).ConfigureAwait(false);
                            }

                            await promptContext.Context.SendActivityAsync(options.RetryPrompt ?? this.RetryPrompt ?? options.Prompt ?? this.InitialPrompt).ConfigureAwait(false);
                            return false;
                        }

                        if (value > options.MaxValue.Value)
                        {
                            if (this.TooLargeResponse != null)
                            {
                                await promptContext.Context.SendActivityAsync(this.TooLargeResponse).ConfigureAwait(false);
                            }

                            await promptContext.Context.SendActivityAsync(options.RetryPrompt ?? this.RetryPrompt ?? options.Prompt ?? this.InitialPrompt).ConfigureAwait(false);
                            return false;
                        }

                        return true;
                    }

                    if (this.NoMatchResponse != null)
                    {
                        await promptContext.Context.SendActivityAsync(this.NoMatchResponse).ConfigureAwait(false);
                    }

                    await promptContext.Context.SendActivityAsync(options.RetryPrompt ?? this.RetryPrompt ?? options.Prompt ?? this.InitialPrompt).ConfigureAwait(false);
                    return false;
                });
            }


            if (isRetry && options.RetryPrompt != null)
            {
                await turnContext.SendActivityAsync(options.RetryPrompt, cancellationToken).ConfigureAwait(false);
            }
            else if (options.Prompt != null)
            {
                await turnContext.SendActivityAsync(options.Prompt, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override Task<PromptRecognizerResult<IList<DateTimeResolution>>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, DateTimePromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<IList<DateTimeResolution>>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.AsMessageActivity();
                var culture = turnContext.Activity.Locale ?? DefaultLocale ?? English;
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

            return Task.FromResult(result);
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
