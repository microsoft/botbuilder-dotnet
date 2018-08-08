// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.Choice;
using static Microsoft.Recognizers.Text.Culture;

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
    public class ConfirmPrompt : Prompt<bool>
    {
        private static readonly Dictionary<string, Tuple<Choice, Choice>> DefaultConfirmChoices = new Dictionary<string, Tuple<Choice, Choice>>()
        {
            { Spanish, new Tuple<Choice, Choice>(new Choice { Value = "Sí" }, new Choice { Value = "No" }) },
            { Dutch, new Tuple<Choice, Choice>(new Choice { Value = "Ja" }, new Choice { Value = "Niet" }) },
            { English, new Tuple<Choice, Choice>(new Choice { Value = "Yes" }, new Choice { Value = "No" }) },
            { French, new Tuple<Choice, Choice>(new Choice { Value = "Oui" }, new Choice { Value = "Non" }) },
            { German, new Tuple<Choice, Choice>(new Choice { Value = "Ja" }, new Choice { Value = "Nein" }) },
            { Japanese, new Tuple<Choice, Choice>(new Choice { Value = "はい" }, new Choice { Value = "いいえ" }) },
            { Portuguese, new Tuple<Choice, Choice>(new Choice { Value = "Sim" }, new Choice { Value = "Não" }) },
            { Chinese, new Tuple<Choice, Choice>(new Choice { Value = "是的" }, new Choice { Value = "不" }) },
        };

        private static readonly Dictionary<string, ChoiceFactoryOptions> DefaultChoiceOptions = new Dictionary<string, ChoiceFactoryOptions>()
        {
            { Spanish, new ChoiceFactoryOptions { InlineSeparator = ", ", InlineOr = " o ", InlineOrMore = ", o ", IncludeNumbers = true } },
            { Dutch, new ChoiceFactoryOptions { InlineSeparator = ", ", InlineOr = " of ", InlineOrMore = ", of ", IncludeNumbers = true } },
            { English, new ChoiceFactoryOptions { InlineSeparator = ", ", InlineOr = " or ", InlineOrMore = ", or ", IncludeNumbers = true } },
            { French, new ChoiceFactoryOptions { InlineSeparator = ", ", InlineOr = " ou ", InlineOrMore = ", ou ", IncludeNumbers = true } },
            { German, new ChoiceFactoryOptions { InlineSeparator = ", ", InlineOr = " oder ", InlineOrMore = ", oder ", IncludeNumbers = true } },
            { Japanese, new ChoiceFactoryOptions { InlineSeparator = "、 ", InlineOr = " または ", InlineOrMore = "、 または ", IncludeNumbers = true } },
            { Portuguese, new ChoiceFactoryOptions { InlineSeparator = ", ", InlineOr = " ou ", InlineOrMore = ", ou ", IncludeNumbers = true } },
            { Chinese, new ChoiceFactoryOptions { InlineSeparator = "， ", InlineOr = " 要么 ", InlineOrMore = "， 要么 ", IncludeNumbers = true } },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmPrompt"/> class.
        /// </summary>
        /// <param name="dialogId">Dialog identifier.</param>
        /// <param name="culture">Culture to use if <c>DialogContext.Context.Activity.Locale</c> property not specified. Defaults to a value of. <code>CultureInfo.CurrentCulture</code>.</param>
        /// <param name="validator">Validator that will be called each time the user responds to the prompt.  If the validator replies with a message no additional retry prompt will be sent.</param>
        /// <param name="defaultLocale">The Locale used by default.</param>
        public ConfirmPrompt(string dialogId, PromptValidator<bool> validator = null, string defaultLocale = null)
            : base(dialogId, validator)
        {
            Style = ListStyle.Auto;
            DefaultLocale = defaultLocale;
        }

        /// <summary>
        /// Gets or sets the style of the yes/no choices rendered to the user when prompting.
        /// <seealso cref="Choices.ListStyle"/>
        /// </summary>
        /// <value>
        /// The style of the yes/no choices rendered to the user when prompting.
        /// </value>
        public ListStyle Style { get; set; }

        public string DefaultLocale { get; set; }

        /// <summary>
        /// Gets or sets additional options passed to the <seealso cref="ChoiceFactory"/>
        /// and used to tweak the style of choices rendered to the user.
        /// </summary>
        /// <value>
        /// Additional options passed to the <seealso cref="ChoiceFactory"/>
        /// and used to tweak the style of choices rendered to the user.
        /// </value>
        public ChoiceFactoryOptions ChoiceOptions { get; set; }

        public Tuple<Choice, Choice> ConfirmChoices { get; set; }

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

            // Determine culture
            var culture = context.Activity.Locale ?? DefaultLocale;
            if (string.IsNullOrEmpty(culture) || !DefaultChoiceOptions.ContainsKey(culture))
            {
                culture = English;
            }

            // Format prompt to send
            IMessageActivity prompt;
            var channelId = context.Activity.ChannelId;
            var choiceOptions = ChoiceOptions ?? DefaultChoiceOptions[culture];
            var confirmChoices = ConfirmChoices ?? DefaultConfirmChoices[culture];
            var choices = new List<Choice> { confirmChoices.Item1, confirmChoices.Item2 };
            if (isRetry && options.RetryPrompt != null)
            {
                prompt = AppendChoices(options.RetryPrompt, channelId, choices, Style, choiceOptions);
            }
            else
            {
                prompt = AppendChoices(options.Prompt, channelId, choices, Style, choiceOptions);
            }

            // Send prompt
            await context.SendActivityAsync(prompt).ConfigureAwait(false);
        }

        protected override Task<PromptRecognizerResult<bool>> OnRecognizeAsync(ITurnContext context, IDictionary<string, object> state, PromptOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var result = new PromptRecognizerResult<bool>();
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Determine culture
                var culture = context.Activity.Locale ?? DefaultLocale;
                if (string.IsNullOrEmpty(culture) || !DefaultChoiceOptions.ContainsKey(culture))
                {
                    culture = English;
                }

                // Recognize utterance
                var message = context.Activity.AsMessageActivity();
                var results = ChoiceRecognizer.RecognizeBoolean(message.Text, culture);
                if (results.Count > 0)
                {
                    var first = results[0];
                    if (bool.TryParse(first.Resolution["value"].ToString(), out var value))
                    {
                        result.Succeeded = true;
                        result.Value = value;
                    }
                }
            }

            return Task.FromResult(result);
        }
    }
}
