// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class TextPromptOptions : PromptOptions
    {
    }

    public class TextPrompt : Prompt<string, TextPromptOptions>
    {
        public TextPrompt(string dialogId = nameof(TextPrompt), PromptValidator<string> validator = null)
            : base(dialogId ?? nameof(TextPrompt), validator)
        {
        }

        /// <summary>
        /// Regex Match expression to match.
        /// </summary>
        private Regex _patternMatcher;

        public string Pattern { get { return _patternMatcher?.ToString(); } set { _patternMatcher = new Regex(value); } }

        public ActivityTemplate NotMatchedActivity { get; set; }

        protected override async Task OnBeforePromptAsync(DialogContext dc, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO: Parametrize to which state to bind.
            await base.OnBeforePromptAsync(dc, isRetry, cancellationToken).ConfigureAwait(false);
            NotMatchedActivity?.Bind(dc.UserState);
        }

        protected override async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, TextPromptOptions options, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
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
                _validator = new PromptValidator<string>(async (promptContext, cancel) =>
                {
                    if (!promptContext.Recognized.Succeeded)
                    {
                        return false;
                    }

                    if (_patternMatcher == null)
                    {
                        return true;
                    }

                    var value = promptContext.Recognized.Value;

                    if (!_patternMatcher.IsMatch(value))
                    {
                        if (this.NotMatchedActivity != null)
                        {
                            await promptContext.Context.SendActivityAsync(this.NotMatchedActivity.Activity).ConfigureAwait(false);
                        }

                        return false;
                    }

                    return true;
                });
            }

            // Retry for template model
            if (isRetry && RetryPrompt != null)
            {
                await turnContext.SendActivityAsync(RetryPrompt.Activity, cancellationToken).ConfigureAwait(false);
            }

            // Backward compatible retry for Options model
            else if (isRetry && options.RetryPrompt != null)
            {
                await turnContext.SendActivityAsync(options.RetryPrompt, cancellationToken).ConfigureAwait(false);
            }

            // Initial prompt for template model
            else if (InitialPrompt != null)
            {
                await turnContext.SendActivityAsync(InitialPrompt.Activity, cancellationToken).ConfigureAwait(false);
            }

            // Backward compatible initial prompt for Options model
            else if (options.Prompt != null)
            {
                await turnContext.SendActivityAsync(options.Prompt, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override Task<PromptRecognizerResult<string>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, TextPromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<string>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.AsMessageActivity();
                if (message.Text != null)
                {
                    result.Succeeded = true;
                    result.Value = message.Text;
                }
            }

            return Task.FromResult(result);
        }
    }
}
