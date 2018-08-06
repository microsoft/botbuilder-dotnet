// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.Number;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class NumberPrompt<T> : Prompt<T>
    {
        public NumberPrompt(string dialogId, PromptValidator<T> validator = null, string defaultLocale = null)
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

        protected override async Task<PromptRecognizerResult<T>> OnRecognizeAsync(ITurnContext context, IDictionary<string, object> state, PromptOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var result = new PromptRecognizerResult<T>();
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var message = context.Activity.AsMessageActivity();
                var culture = context.Activity.Locale ?? DefaultLocale ?? English;
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

            return result;
        }
    }
}
