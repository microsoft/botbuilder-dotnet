// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Prompts a user to confirm something with a yes/no response.
    ///
    /// <remarks>By default the prompt will return to the calling dialog a `boolean` representing the users
    /// selection.
    /// When used with your bots 'DialogSet' you can simply add a new instance of the prompt as a named
    /// dialog using <code>DialogSet.Add()</code>. You can then start the prompt from a waterfall step using either
    /// <code>DialogContext.Begin()</code> or <code>DialogContext.Prompt()</code>. The user will be prompted to answer a
    /// 'yes/no' or 'true/false' question and the users response will be passed as an argument to the
    /// callers next waterfall step
    /// </remarks>
    /// </summary>
    public class ConfirmPrompt : Prompt<ConfirmResult>
    {
        private ConfirmPromptInternal _prompt;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmPrompt"/> class.
        /// </summary>
        /// <param name="culture">Culture to use if <c>DialogContext.Context.Activity.Locale</c> property not specified. Defaults to a value of. <code>CultureInfo.CurrentCulture</code>.</param>
        /// <param name="validator">Validator that will be called each time the user responds to the prompt.  If the validator replies with a message no additional retry prompt will be sent.</param>
        public ConfirmPrompt(string culture, PromptValidator<ConfirmResult> validator = null) => _prompt = new ConfirmPromptInternal(culture, validator);

        /// <summary>
        /// Gets or sets the style of the yes/no choices rendered to the user when prompting.
        /// <seealso cref="Choices.ListStyle"/>
        /// </summary>
        /// <value>
        /// The style of the yes/no choices rendered to the user when prompting.
        /// </value>
        public ListStyle Style
        {
            get { return _prompt.Style; }
            set { _prompt.Style = value; }
        }

        /// <summary>
        /// Gets or sets additional options passed to the <seealso cref="ChoiceFactory"/>
        /// and used to tweak the style of choices rendered to the user.
        /// </summary>
        /// <value>
        /// Additional options passed to the <seealso cref="ChoiceFactory"/>
        /// and used to tweak the style of choices rendered to the user.
        /// </value>
        public ChoiceFactoryOptions ChoiceOptions
        {
            get { return _prompt.ChoiceOptions;  }
            set { _prompt.ChoiceOptions = value;  }
        }

        protected override async Task OnPromptAsync(DialogContext dc, PromptOptions options, bool isRetry)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (isRetry)
            {
                if (options.RetryPromptActivity != null)
                {
                    await _prompt.PromptAsync(dc.Context, options.RetryPromptActivity.AsMessageActivity());
                }
                else if (options.RetryPromptString != null)
                {
                    await _prompt.PromptAsync(dc.Context, options.RetryPromptString, options.RetrySpeak);
                }
            }
            else
            {
                if (options.PromptActivity != null)
                {
                    await _prompt.PromptAsync(dc.Context, options.PromptActivity);
                }
                else if (options.PromptString != null)
                {
                    await _prompt.PromptAsync(dc.Context, options.PromptString, options.Speak);
                }
            }
        }

        protected override async Task<ConfirmResult> OnRecognizeAsync(DialogContext dc, PromptOptions options)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await _prompt.RecognizeAsync(dc.Context);
        }
    }
}
