// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Prompts;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class TextPrompt : Prompt<TextResult>
    {
        private Builder.Prompts.TextPrompt _prompt;

        public TextPrompt(PromptValidator<TextResult> validator = null)
        {
            _prompt = new Builder.Prompts.TextPrompt(validator);
        }

        protected override Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry)
        {
            if (isRetry)
            {
                if (options.RetryPromptActivity != null)
                {
                    return _prompt.Prompt(dc.Context, options.RetryPromptActivity.AsMessageActivity());
                }
                if (options.RetryPromptString != null)
                {
                    return _prompt.Prompt(dc.Context, options.RetryPromptString, options.RetrySpeak);
                }
            }
            else
            {
                if (options.PromptActivity != null)
                {
                    return _prompt.Prompt(dc.Context, options.PromptActivity);
                }
                if (options.PromptString != null)
                {
                    return _prompt.Prompt(dc.Context, options.PromptString, options.Speak);
                }
            }
            return Task.CompletedTask;
        }

        protected override async Task<TextResult> OnRecognize(DialogContext dc, PromptOptions options)
        {
            return await _prompt.Recognize(dc.Context);
        }
    }
}
