// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Dialogs.Prompts.PromptCultureModels;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Prompts a user to select from a list of choices.
    /// </summary>
    public class ChoicePrompt : Prompt<FoundChoice>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChoicePrompt"/> class.
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
        public ChoicePrompt(string dialogId, PromptValidator<FoundChoice> validator = null, string defaultLocale = null)
            : base(dialogId, validator)
        {
            Style = ListStyle.Auto;
            DefaultLocale = defaultLocale;
        }

        /// <summary>
        /// Gets or sets a dictionary used for custom prompt display based on user-provided locale settings.
        /// </summary>
        /// <value>The dictionary used for custom prompt display based on user-provided locale settings.</value>
        /// <remarks>
        /// If set, this replaces DefaultChoiceOptions. Because this is static, it is not thread-safe.
        /// Usage: ChoicePrompt.CustomLocaleOptions = new Dictionary<string, ChoiceFactoryOptions>()
        ///         { "en-US", new ChoiceFactoryOptions {...} }
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:Documentation text should end with a period", Justification = "Doesn't like `Dictionary<string`")]
        public static Dictionary<string, ChoiceFactoryOptions> CustomLocaleOptions { get; set; }

        /// <summary>
        /// Gets or sets the style to use when presenting the prompt to the user.
        /// </summary>
        /// <value>The style to use when presenting the prompt to the user.</value>
        public ListStyle Style { get; set; }

        /// <summary>
        /// Gets or sets the default locale used to determine language-specific behavior of the prompt.
        /// </summary>
        /// <value>The default locale used to determine language-specific behavior of the prompt.</value>
        public string DefaultLocale { get; set; }

        /// <summary>
        /// Gets or sets additional options passed to the <see cref="ChoiceFactory"/> and used to tweak
        /// the style of choices rendered to the user.
        /// </summary>
        /// <value>Additional options for presenting the set of choices.</value>
        public ChoiceFactoryOptions ChoiceOptions { get; set; }

        /// <summary>
        /// Gets or sets additional options passed to the underlying
        /// <see cref="ChoiceRecognizers.RecognizeChoices(string, IList{Choice}, FindChoicesOptions)"/> method.
        /// </summary>
        /// <value>Options to control the recognition strategy.</value>
        public FindChoicesOptions RecognizerOptions { get; set; }

        /// <summary>
        /// Gets a dictionary of Default ChoiceOptions based on Microsoft.Bot.Builder.Dialogs.Prompts.PromptCultureModels.GetSupportedCultures.
        /// </summary>
        private static Dictionary<string, ChoiceFactoryOptions> DefaultChoiceOptions
        {
            get
            {
                if (CustomLocaleOptions == null || CustomLocaleOptions.Count == 0)
                {
                    var defaults = new Dictionary<string, ChoiceFactoryOptions>();
                    foreach (var culture in GetSupportedCultures())
                    {
                        defaults.Add(culture.Locale, new ChoiceFactoryOptions { InlineSeparator = culture.Separator, InlineOr = culture.InlineOr, InlineOrMore = culture.InlineOrMore, IncludeNumbers = true });
                    }

                    return defaults;
                }

                return CustomLocaleOptions;
            }
        }

        /// <summary>
        /// Prompts the user for input.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="state">Contains state for the current instance of the prompt on the dialog stack.</param>
        /// <param name="options">A prompt options object constructed from the options initially provided
        /// in the call to <see cref="DialogContext.PromptAsync(string, PromptOptions, CancellationToken)"/>.</param>
        /// <param name="isRetry">true  if this is the first time this prompt dialog instance
        /// on the stack is prompting the user for input; otherwise, false.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Determine culture
            var culture = MapToNearestLanguage(turnContext.Activity.Locale ?? DefaultLocale);
            if (string.IsNullOrEmpty(culture) || !DefaultChoiceOptions.ContainsKey(culture))
            {
                culture = English.Locale;
            }

            // Format prompt to send
            IMessageActivity prompt;
            var choices = options.Choices ?? new List<Choice>();
            var channelId = turnContext.Activity.ChannelId;
            var choiceOptions = ChoiceOptions ?? DefaultChoiceOptions[culture];
            var choiceStyle = options.Style ?? Style;
            if (isRetry && options.RetryPrompt != null)
            {
                prompt = AppendChoices(options.RetryPrompt, channelId, choices, choiceStyle, choiceOptions);
            }
            else
            {
                prompt = AppendChoices(options.Prompt, channelId, choices, choiceStyle, choiceOptions);
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
        protected override Task<PromptRecognizerResult<FoundChoice>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var choices = options.Choices ?? new List<Choice>();

            var result = new PromptRecognizerResult<FoundChoice>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var activity = turnContext.Activity;
                var utterance = activity.Text;
                var opt = RecognizerOptions ?? new FindChoicesOptions();
                opt.Locale = MapToNearestLanguage(activity.Locale ?? opt.Locale ?? DefaultLocale ?? English.Locale);
                var results = ChoiceRecognizers.RecognizeChoices(utterance, choices, opt);
                if (results != null && results.Count > 0)
                {
                    result.Succeeded = true;
                    result.Value = results[0].Resolution;
                }
            }

            return Task.FromResult(result);
        }
    }
}
