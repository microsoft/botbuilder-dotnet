// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Prompts;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    public enum ChoicePromptStyle
    {
        /// <summary>
        /// Don't include any choices for prompt.
        /// </summary>
        None,

        /// <summary>
        /// Automatically select the appropriate style for the current channel.
        /// </summary>
        Auto,

        /// <summary>
        /// Add choices to prompt as an inline list.
        /// </summary>
        InLine,

        /// <summary>
        /// Add choices to prompt as a numbered list.
        /// </summary>
        List,

        /// <summary>
        /// Add choices to prompt as suggested actions.
        /// </summary>
        SuggestedAction
    }

    public class ChoicePrompt : Prompt<ChoiceResult<string>>
    {
        private Prompts.ChoicePrompt _prompt;

        /// <summary>
        /// Style of choices sent to user. Defaults to ChoicePromptStyle.Auto.
        /// </summary>
        public ChoicePromptStyle Style { get; set; }

        /// <summary>
        /// Additional options used to configure the output of the choice factory.
        /// </summary>
        public IChoiceFactoryOptions ChoiceOptions { get; set; }

        /// <summary>
        /// if true, inline and list style choices will be prefixed with the index of the
        /// choice as in "1. choice". If false, the list style will use a bulleted list instead.
        /// The default value is true.
        /// </summary>
        public bool IncludeNumbers { get; set; }

        public ChoicePrompt(string culture, IList<Choice> choices, PromptValidator<ChoiceResult<string>> validator = null, bool allowPartialMatch = false, int maxDistance = 2)
        {
            _prompt = new Prompts.ChoicePrompt(culture, choices, validator, allowPartialMatch, maxDistance);
            Style = ChoicePromptStyle.Auto;
            ChoiceOptions = new DefaultChoiceFactoryOptions();
            IncludeNumbers = true;
        }

        public ChoicePrompt(string culture, IEnumerable<string> choices, PromptValidator<ChoiceResult<string>> validator = null, bool allowPartialMatch = false, int maxDistance = 2)
            : this(culture, choices.ToChoices(), validator, allowPartialMatch, maxDistance)
        { }

        protected override Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (isRetry)
            {
                if (options.RetryPromptActivity != null)
                {
                    return _prompt.Prompt(dc.Context, options.RetryPromptActivity.AsMessageActivity());
                }
                if (options.RetryPromptString != null)
                {
                    return _prompt.Prompt(dc.Context, StyledChoices(dc.Context, options.RetryPromptString, options.RetrySpeak));
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
                    return _prompt.Prompt(dc.Context, StyledChoices(dc.Context, options.PromptString, options.Speak));
                }
            }
            return Task.CompletedTask;
        }

        protected override async Task<ChoiceResult<string>> OnRecognize(DialogContext dc, PromptOptions options)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return await _prompt.Recognize(dc.Context);
        }

        private Schema.IMessageActivity StyledChoices(ITurnContext context, string text, string speak)
        {
            switch (Style)
            {
                case ChoicePromptStyle.Auto:
                    return _prompt.Choices.ChoicesForChannel(context, text, speak, ChoiceOptions, IncludeNumbers);
                case ChoicePromptStyle.InLine:
                    return _prompt.Choices.ChoicesToInline(text, speak, ChoiceOptions, IncludeNumbers);
                case ChoicePromptStyle.List:
                    return _prompt.Choices.ChoicesToList(text, speak, IncludeNumbers);
                case ChoicePromptStyle.SuggestedAction:
                    return _prompt.Choices.SuggestedAction(text, speak);
                case ChoicePromptStyle.None:
                default:
                    return MessageFactory.Text(text, speak, Schema.InputHints.ExpectingInput);
            }
        }
    }
}
