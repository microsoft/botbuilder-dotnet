// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class ChoicePromptOptions : PromptOptions
    {
        public List<Choice> Choices
        {
            get { return GetProperty<List<Choice>>(nameof(Choices)); }
            set { this[nameof(Choices)] = value; }
        }
    }

    public class ChoicePrompt : Prompt<ChoiceResult>
    {
        private ChoicePromptInternal _prompt;

        public ChoicePrompt(string culture, PromptValidator<ChoiceResult> validator = null)
        {
            _prompt = new ChoicePromptInternal(culture, validator);
        }

        public ListStyle Style
        {
            get { return _prompt.Style; }
            set { _prompt.Style = value; }
        }

        public string Culture
        {
            get { return _prompt.Culture; }
            set { _prompt.Culture = value; }
        }

        public ChoiceFactoryOptions ChoiceOptions
        {
            get { return _prompt.ChoiceOptions; }
            set { _prompt.ChoiceOptions = value; }
        }

        public FindChoicesOptions RecognizerOptions
        {
            get { return _prompt.RecognizerOptions; }
            set { _prompt.RecognizerOptions = value; }
        }

        protected override async Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var choices = (options as ChoicePromptOptions)?.Choices ?? new List<Choice>();

            if (isRetry)
            {
                if (options.RetryPromptActivity != null)
                {
                    await _prompt.Prompt(dc.Context, options.RetryPromptActivity, options.Speak);
                }
                else if (options.RetryPromptString != null)
                {
                    await _prompt.Prompt(dc.Context, choices, options.RetryPromptString, options.RetrySpeak);
                }
            }
            else
            {
                if (options.PromptActivity != null)
                {
                    await _prompt.Prompt(dc.Context, options.PromptActivity, options.Speak);
                }
                else if (options.PromptString != null)
                {
                    await _prompt.Prompt(dc.Context, choices, options.PromptString, options.Speak);
                }
            }
        }

        protected override async Task<ChoiceResult> OnRecognize(DialogContext dc, PromptOptions options)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var choices = (options as ChoicePromptOptions)?.Choices ?? new List<Choice>();

            return await _prompt.Recognize(dc.Context, choices);
        }
    }
}
