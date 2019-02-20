// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Prompts;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.Number;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class NumberPromptOptions<TNumber> : PromptOptions
        where TNumber : struct, IComparable<TNumber>
    {
    }

    public class NumberPrompt<TNumber> : Prompt<TNumber, NumberPromptOptions<TNumber>>, IRangePromptOptions<TNumber>
        where TNumber : struct, IComparable<TNumber>
    {
        public NumberPrompt(string dialogId = nameof(NumberPrompt<TNumber>), PromptValidator<TNumber> validator = null, string defaultLocale = null)
            : base(dialogId ?? nameof(NumberPrompt<TNumber>), validator)
        {
            DefaultLocale = defaultLocale;
            MinValue = (TNumber)typeof(TNumber).GetField(nameof(MinValue)).GetValue(null);
            MaxValue = (TNumber)typeof(TNumber).GetField(nameof(MaxValue)).GetValue(null);

            this.TooSmallResponse = this.DefineMessageActivityProperty(nameof(TooSmallResponse));
            this.TooLargeResponse = this.DefineMessageActivityProperty(nameof(TooLargeResponse));
        }

        public string DefaultLocale { get; set; }

        public TNumber MinValue { get; set; }

        public TNumber MaxValue { get; set; }

        public ITemplate<IMessageActivity> TooSmallResponse { get; set; }

        public ITemplate<IMessageActivity> TooLargeResponse { get; set; }

        protected override async Task OnBeforePromptAsync(DialogContext dc, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO: Parametrize to which state to bind.
            await base.OnBeforePromptAsync(dc, isRetry, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, NumberPromptOptions<TNumber> options, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (_validator == null)
            {
                _validator = new PromptValidator<TNumber>(async (promptContext, cancel) =>
                {
                    if (!promptContext.Recognized.Succeeded)
                    {
                        if (options.RetryPrompt != null)
                        {
                            await promptContext.Context.SendActivityAsync(options.RetryPrompt).ConfigureAwait(false);
                            return false;
                        }

                        var noMatch = await this.NoMatchResponse.BindToData(turnContext, state).ConfigureAwait(false);
                        if (noMatch != null)
                        {
                            await promptContext.Context.SendActivityAsync(noMatch).ConfigureAwait(false);
                        }

                        var retry = await this.RetryPrompt.BindToData(turnContext, state).ConfigureAwait(false);
                        await promptContext.Context.SendActivityAsync(retry).ConfigureAwait(false);
                        return false;
                    }

                    var result = (IComparable<TNumber>)promptContext.Recognized.Value;

                    if (result.CompareTo(MinValue) < 0)
                    {
                        var tooSmall = await this.TooSmallResponse.BindToData(turnContext, state).ConfigureAwait(false);
                        if (tooSmall != null)
                        {
                            await promptContext.Context.SendActivityAsync(tooSmall).ConfigureAwait(false);
                        }

                        if (options.RetryPrompt != null)
                        {
                            await promptContext.Context.SendActivityAsync(options.RetryPrompt).ConfigureAwait(false);
                            return false;
                        }

                        var retry = await this.RetryPrompt.BindToData(turnContext, state).ConfigureAwait(false);
                        await promptContext.Context.SendActivityAsync(retry).ConfigureAwait(false);
                        return false;
                    }

                    if (result.CompareTo(MaxValue) > 0)
                    {
                        var tooLarge = await this.TooLargeResponse.BindToData(turnContext, state).ConfigureAwait(false);
                        if (tooLarge != null)
                        {
                            await promptContext.Context.SendActivityAsync(tooLarge).ConfigureAwait(false);
                        }

                        if (options.RetryPrompt != null)
                        {
                            await promptContext.Context.SendActivityAsync(options.RetryPrompt).ConfigureAwait(false);
                            return false;
                        }

                        var retry = await this.RetryPrompt.BindToData(turnContext, state).ConfigureAwait(false);
                        await promptContext.Context.SendActivityAsync(retry).ConfigureAwait(false);
                        return false;
                    }

                    return true;
                });
            }

            if (isRetry && options.RetryPrompt != null)
            {
                // backwards compat
                await turnContext.SendActivityAsync(options.RetryPrompt, cancellationToken).ConfigureAwait(false);
            }
            else if (isRetry)
            {
                // new model
                var retry = await this.RetryPrompt.BindToData(turnContext, state).ConfigureAwait(false);
                await turnContext.SendActivityAsync(retry).ConfigureAwait(false);
            }
            else if (options.Prompt != null)
            {
                // Backward compatible initial prompt for Options model
                await turnContext.SendActivityAsync(options.Prompt, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Initial prompt for template model
                var intialPrompt = await this.InitialPrompt.BindToData(turnContext, state).ConfigureAwait(false);
                await turnContext.SendActivityAsync(intialPrompt).ConfigureAwait(false);
            }
        }

        protected override Task<PromptRecognizerResult<TNumber>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, NumberPromptOptions<TNumber> options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<TNumber>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.AsMessageActivity();
                var culture = turnContext.Activity.Locale ?? DefaultLocale ?? English;
                var results = NumberRecognizer.RecognizeNumber(message.Text, culture);
                if (results.Count > 0)
                {
                    // Try to parse value based on type
                    var text = results[0].Resolution["value"].ToString();
                    if (typeof(TNumber) == typeof(float))
                    {
                        if (float.TryParse(text, out var value))
                        {
                            result.Succeeded = true;
                            result.Value = (TNumber)(object)value;
                        }
                    }
                    else if (typeof(TNumber) == typeof(int))
                    {
                        if (int.TryParse(text, out var value))
                        {
                            result.Succeeded = true;
                            result.Value = (TNumber)(object)value;
                        }
                    }
                    else if (typeof(TNumber) == typeof(long))
                    {
                        if (long.TryParse(text, out var value))
                        {
                            result.Succeeded = true;
                            result.Value = (TNumber)(object)value;
                        }
                    }
                    else if (typeof(TNumber) == typeof(double))
                    {
                        if (double.TryParse(text, out var value))
                        {
                            result.Succeeded = true;
                            result.Value = (TNumber)(object)value;
                        }
                    }
                    else if (typeof(TNumber) == typeof(decimal))
                    {
                        if (decimal.TryParse(text, out var value))
                        {
                            result.Succeeded = true;
                            result.Value = (TNumber)(object)value;
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
