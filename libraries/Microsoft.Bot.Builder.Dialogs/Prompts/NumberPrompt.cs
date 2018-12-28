// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.Number;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class NumberPrompt<T> : Prompt<T>
        where T : struct
    {
        public NumberPrompt(string dialogId, PromptValidator<T> validator = null, string defaultLocale = null)
            : base(dialogId, validator)
        {
            DefaultLocale = defaultLocale;
        }

        public string DefaultLocale { get; set; }

        protected override async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
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

        protected override Task<PromptRecognizerResult<T>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<T>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.AsMessageActivity();
                var culture = turnContext.Activity.Locale ?? DefaultLocale ?? English;
                var results = NumberRecognizer.RecognizeNumber(message.Text, culture);
                if (results.Count > 0)
                {
                    // Try to parse value based on type
                    var text = results[0].Resolution["value"].ToString();
                    if (typeof(T) == typeof(float))
                    {
                        if (float.TryParse(text, out var value))
                        {
                            result.Succeeded = true;
                            result.Value = (T)(object)value;
                        }
                    }
                    else if (typeof(T) == typeof(int))
                    {
                        if (int.TryParse(text, out var value))
                        {
                            result.Succeeded = true;
                            result.Value = (T)(object)value;
                        }
                    }
                    else if (typeof(T) == typeof(long))
                    {
                        if (long.TryParse(text, out var value))
                        {
                            result.Succeeded = true;
                            result.Value = (T)(object)value;
                        }
                    }
                    else if (typeof(T) == typeof(double))
                    {
                        if (double.TryParse(text, out var value))
                        {
                            result.Succeeded = true;
                            result.Value = (T)(object)value;
                        }
                    }
                    else if (typeof(T) == typeof(decimal))
                    {
                        if (decimal.TryParse(text, out var value))
                        {
                            result.Succeeded = true;
                            result.Value = (T)(object)value;
                        }
                    }
                    else
                    {
                        throw new NotSupportedException($"NumberPrompt: type argument T of type 'typeof(T)' is not supported");
                    }
                }
            }

            return Task.FromResult(result);
        }
    }
}
