// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.Choice;
using static Microsoft.Bot.Builder.Dialogs.Prompts.PromptCultureModels;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Prompts a user to confirm something with a yes/no response.
    /// </summary>
    public class ConfirmPrompt : Prompt<bool>
    {
        /// <summary>
        /// A dictionary of Default Choices based on <seealso cref="GetSupportedCultures"/>.
        /// Can be replaced by user using the constructor that contains choiceDefaults.
        /// </summary>
        private readonly Dictionary<string, (Choice, Choice, ChoiceFactoryOptions)> _choiceDefaults;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmPrompt"/> class.
        /// </summary>
        /// <param name="dialogId">The ID to assign to this prompt.</param>
        /// <param name="validator">Optional, a <see cref="PromptValidator{FoundChoice}"/> that contains additional,
        /// custom validation for this prompt.</param>
        /// <param name="defaultLocale">Optional, the default locale used to determine language-specific behavior of the prompt.
        /// The locale is a 2, 3, or 4 character ISO 639 code that represents a language or language family.</param>
        /// <remarks>The value of <paramref name="dialogId"/> must be unique within the
        /// <see cref="DialogSet"/> or <see cref="ComponentDialog"/> to which the prompt is added.
        /// <para>If the <see cref="Activity.Locale"/>
        /// of the <see cref="DialogContext"/>.<see cref="DialogContext.Context"/>.<see cref="ITurnContext.Activity"/>
        /// is specified, then that local is used to determine language specific behavior; otherwise
        /// the <paramref name="defaultLocale"/> is used. US-English is the used if no language or
        /// default locale is available, or if the language or locale is not otherwise supported.</para></remarks>
        public ConfirmPrompt(string dialogId, PromptValidator<bool> validator = null, string defaultLocale = null)
            : this(
                dialogId,
                new Dictionary<string, (Choice, Choice, ChoiceFactoryOptions)>(
                    GetSupportedCultures().ToDictionary(
                        culture => culture.Locale, culture =>
                        (new Choice(culture.YesInLanguage),
                            new Choice(culture.NoInLanguage),
                            new ChoiceFactoryOptions(culture.Separator, culture.InlineOr, culture.InlineOrMore, true)))),
                validator,
                defaultLocale)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmPrompt"/> class.
        /// </summary>
        /// <param name="dialogId">The ID to assign to this prompt.</param>
        /// <param name="validator">Optional, a <see cref="PromptValidator{FoundChoice}"/> that contains additional,
        /// custom validation for this prompt.</param>
        /// <param name="defaultLocale">Optional, the default locale used to determine language-specific behavior of the prompt.
        /// The locale is a 2, 3, or 4 character ISO 639 code that represents a language or language family.</param>
        /// <param name="choiceDefaults">Overrides the dictionary of Bot Framework SDK-supported _choiceDefaults (for prompt localization).
        /// Must be passed in to each ConfirmPrompt that needs the custom choice defaults.</param>
        /// <remarks>The value of <paramref name="dialogId"/> must be unique within the
        /// <see cref="DialogSet"/> or <see cref="ComponentDialog"/> to which the prompt is added.
        /// <para>If the <see cref="Activity.Locale"/>
        /// of the <see cref="DialogContext"/>.<see cref="DialogContext.Context"/>.<see cref="ITurnContext.Activity"/>
        /// is specified, then that local is used to determine language specific behavior; otherwise
        /// the <paramref name="defaultLocale"/> is used. US-English is the used if no language or
        /// default locale is available, or if the language or locale is not otherwise supported.</para></remarks>
        public ConfirmPrompt(string dialogId, Dictionary<string, (Choice, Choice, ChoiceFactoryOptions)> choiceDefaults, PromptValidator<bool> validator = null, string defaultLocale = null)
            : base(dialogId, validator)
        {
            Style = ListStyle.Auto;
            DefaultLocale = defaultLocale;
            _choiceDefaults = choiceDefaults;
        }

        /// <summary>
        /// Gets or sets the style of the yes/no choices rendered to the user when prompting.
        /// </summary>
        /// <value>
        /// The style of the yes/no choices rendered to the user when prompting.
        /// </value>
        public ListStyle Style { get; set; }

        /// <summary>
        /// Gets or sets the default locale used to determine language-specific behavior of the prompt.
        /// </summary>
        /// <value>The default locale used to determine language-specific behavior of the prompt.</value>
        public string DefaultLocale { get; set; }

        /// <summary>
        /// Gets or sets additional options passed to the <seealso cref="ChoiceFactory"/>
        /// and used to tweak the style of choices rendered to the user.
        /// </summary>
        /// <value>Additional options for presenting the set of choices.</value>
        public ChoiceFactoryOptions ChoiceOptions { get; set; }

        /// <summary>
        /// Gets or sets the yes and no <see cref="Choice"/> for the prompt.
        /// </summary>
        /// <value>The yes and no <see cref="Choice"/> for the prompt.</value>
        public Tuple<Choice, Choice> ConfirmChoices { get; set; }

        /// <summary>
        /// Prompts the user for input.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="state">Contains state for the current instance of the prompt on the dialog stack.</param>
        /// <param name="options">A prompt options object constructed from the options initially provided
        /// in the call to <see cref="DialogContext.PromptAsync(string, PromptOptions, CancellationToken)"/>.</param>
        /// <param name="isRetry">true if this is the first time this prompt dialog instance
        /// on the stack is prompting the user for input; otherwise, false.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task OnPromptAsync(
            ITurnContext turnContext,
            IDictionary<string, object> state,
            PromptOptions options,
            bool isRetry,
            CancellationToken cancellationToken = default)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Format prompt to send
            IMessageActivity prompt;
            var channelId = turnContext.Activity.ChannelId;
            var culture = DetermineCulture(turnContext.Activity);
            var defaults = _choiceDefaults[culture];
            var choiceOptions = ChoiceOptions ?? defaults.Item3;
            var confirmChoices = ConfirmChoices ?? Tuple.Create(defaults.Item1, defaults.Item2);
            var choices = new List<Choice> { confirmChoices.Item1, confirmChoices.Item2 };
            var style = options.Style ?? Style;
            if (isRetry && options.RetryPrompt != null)
            {
                prompt = AppendChoices(options.RetryPrompt, channelId, choices, style, choiceOptions);
            }
            else
            {
                prompt = AppendChoices(options.Prompt, channelId, choices, style, choiceOptions);
            }

            // Send prompt
            await turnContext.SendActivityAsync(prompt, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to recognize the user's input.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="state">Contains state for the current instance of the prompt on the dialog stack.</param>
        /// <param name="options">A prompt options object constructed from the options initially provided
        /// in the call to <see cref="DialogContext.PromptAsync(string, PromptOptions, CancellationToken)"/>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result describes the result of the recognition attempt.</remarks>
        protected override Task<PromptRecognizerResult<bool>> OnRecognizeAsync(
            ITurnContext turnContext,
            IDictionary<string, object> state,
            PromptOptions options,
            CancellationToken cancellationToken = default)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<bool>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Recognize utterance
                var utterance = turnContext.Activity.AsMessageActivity().Text;
                if (string.IsNullOrEmpty(utterance))
                {
                    return Task.FromResult(result);
                }

                var culture = DetermineCulture(turnContext.Activity);
                var results = ChoiceRecognizer.RecognizeBoolean(utterance, culture);
                if (results.Count > 0)
                {
                    var first = results[0];
                    if (bool.TryParse(first.Resolution["value"].ToString(), out var value))
                    {
                        result.Succeeded = true;
                        result.Value = value;
                    }
                }
                else
                {
                    // First check whether the prompt was sent to the user with numbers - if it was we should recognize numbers
                    var defaults = _choiceDefaults[culture];
                    var choiceOptions = ChoiceOptions ?? defaults.Item3;

                    // This logic reflects the fact that IncludeNumbers is nullable and True is the default set in Inline style
                    if (!choiceOptions.IncludeNumbers.HasValue || choiceOptions.IncludeNumbers.Value)
                    {
                        // The text may be a number in which case we will interpret that as a choice.
                        var confirmChoices = ConfirmChoices ?? Tuple.Create(defaults.Item1, defaults.Item2);
                        var choices = new List<Choice> { confirmChoices.Item1, confirmChoices.Item2 };
                        var secondAttemptResults = ChoiceRecognizers.RecognizeChoices(utterance, choices);
                        if (secondAttemptResults.Count > 0)
                        {
                            result.Succeeded = true;
                            result.Value = secondAttemptResults[0].Resolution.Index == 0;
                        }
                    }
                }
            }

            return Task.FromResult(result);
        }

        private string DetermineCulture(Activity activity)
        {
            var culture = MapToNearestLanguage(activity.Locale ?? DefaultLocale ?? English.Locale);
            if (string.IsNullOrEmpty(culture) || !_choiceDefaults.ContainsKey(culture))
            {
                culture = English.Locale;
            }

            return culture;
        }
    }
}
