// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
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
        private static readonly Dictionary<string, (Choice, Choice, ChoiceFactoryOptions)> ChoiceDefaults = new Dictionary<string, (Choice, Choice, ChoiceFactoryOptions)>()
        {
            { Spanish, (new Choice("Sí"), new Choice("No"), new ChoiceFactoryOptions(", ", " o ", ", o ", true)) },
            { Dutch, (new Choice("Ja"), new Choice("Nee"), new ChoiceFactoryOptions(", ", " of ", ", of ", true)) },
            { English, (new Choice("Yes"), new Choice("No"), new ChoiceFactoryOptions(", ", " or ", ", or ", true)) },
            { French, (new Choice("Oui"), new Choice("Non"), new ChoiceFactoryOptions(", ", " ou ", ", ou ", true)) },
            { German, (new Choice("Ja"), new Choice("Nein"), new ChoiceFactoryOptions(", ", " oder ", ", oder ", true)) },
            { Japanese, (new Choice("はい"), new Choice("いいえ"), new ChoiceFactoryOptions("、 ", " または ", "、 または ", true)) },
            { Portuguese, (new Choice("Sim"), new Choice("Não"), new ChoiceFactoryOptions(", ", " ou ", ", ou ", true)) },
            { Chinese, (new Choice("是的"), new Choice("不"), new ChoiceFactoryOptions("， ", " 要么 ",  "， 要么 ", true)) },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmPrompt"/> class.
        /// </summary>
        /// <param name="dialogId">Dialog identifier.</param>
        /// <param name="validator">Validator that will be called each time the user responds to the prompt.
        /// If the validator replies with a message no additional retry prompt will be sent.</param>
        /// <param name="defaultLocale">The default culture or locale to use if the <see cref="Activity.Locale"/>
        /// of the <see cref="DialogContext"/>.<see cref="DialogContext.Context"/>.<see cref="ITurnContext.Activity"/>
        /// is not specified.</param>
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

            // Format prompt to send
            IMessageActivity prompt;
            var channelId = turnContext.Activity.ChannelId;
            var culture = DetermineCulture(turnContext.Activity);
            var defaults = ChoiceDefaults[culture];
            var choiceOptions = ChoiceOptions ?? defaults.Item3;
            var confirmChoices = ConfirmChoices ?? Tuple.Create(defaults.Item1, defaults.Item2);
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
            await turnContext.SendActivityAsync(prompt, cancellationToken).ConfigureAwait(false);
        }

        protected override Task<PromptRecognizerResult<bool>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<bool>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Recognize utterance
                var message = turnContext.Activity.AsMessageActivity();
                var culture = DetermineCulture(turnContext.Activity);
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
                else
                {
                    // First check whether the prompt was sent to the user with numbers - if it was we should recognize numbers
                    var defaults = ChoiceDefaults[culture];
                    var choiceOptions = ChoiceOptions ?? defaults.Item3;

                    // This logic reflects the fact that IncludeNumbers is nullable and True is the default set in Inline style
                    if (!choiceOptions.IncludeNumbers.HasValue || choiceOptions.IncludeNumbers.Value)
                    {
                        // The text may be a number in which case we will interpret that as a choice.
                        var confirmChoices = ConfirmChoices ?? Tuple.Create(defaults.Item1, defaults.Item2);
                        var choices = new List<Choice> { confirmChoices.Item1, confirmChoices.Item2 };
                        var secondAttemptResults = ChoiceRecognizers.RecognizeChoices(message.Text, choices);
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
            var culture = activity.Locale ?? DefaultLocale;
            if (string.IsNullOrEmpty(culture) || !ChoiceDefaults.ContainsKey(culture))
            {
                culture = English;
            }

            return culture;
        }
    }
}
