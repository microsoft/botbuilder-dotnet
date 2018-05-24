// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Prompts.Results;
using System;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class ConfirmPrompt : Prompt<ConfirmResult>
    {
        private Prompts.ConfirmPrompt _prompt;

        public ConfirmPrompt(string culture, PromptValidator<ConfirmResult> validator = null)
        {
            _prompt = new Prompts.ConfirmPrompt(culture, validator);
        }

        protected override async Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (isRetry)
            {
                if (options.RetryPromptActivity != null)
                {
                    await _prompt.Prompt(dc.Context, options.RetryPromptActivity.AsMessageActivity());
                }
                else if (options.RetryPromptString != null)
                {
                    await _prompt.Prompt(dc.Context, options.RetryPromptString, options.RetrySpeak);
                }
            }
            else
            {
                if (options.PromptActivity != null)
                {
                    await _prompt.Prompt(dc.Context, options.PromptActivity);
                }
                else if (options.PromptString != null)
                {
                    await _prompt.Prompt(dc.Context, options.PromptString, options.Speak);
                }
            }
        }

        protected override async Task<ConfirmResult> OnRecognize(DialogContext dc, PromptOptions options)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return await _prompt.Recognize(dc.Context);
        }
    }
}