// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class TextPrompt : Prompt<TextResult>
    {
        private TextPromptInternal _prompt;

        public TextPrompt(PromptValidator<TextResult> validator = null)
        {
            _prompt = new TextPromptInternal(validator);
        }

        protected override Task OnPromptAsync(DialogContext dc, PromptOptions options, bool isRetry)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return dc.Context.SendActivityAsync(PromptMessageFactory.CreateActivity(options, isRetry));
        }

        protected override async Task<TextResult> OnRecognizeAsync(DialogContext dc, PromptOptions options)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await _prompt.RecognizeAsync(dc.Context).ConfigureAwait(false);
        }
    }
}
