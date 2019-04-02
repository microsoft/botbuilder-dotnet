// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class ChoicePrompt : Prompt<FoundChoice>
    {
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

        public ChoicePrompt(string dialogId, PromptValidator<FoundChoice> validator = null, string defaultLocale = null)
            : base(dialogId, validator)
        {
            Style = ListStyle.Auto;
            DefaultLocale = defaultLocale;
        }

        public ListStyle Style { get; set; }

        public string DefaultLocale { get; set; }

        public ChoiceFactoryOptions ChoiceOptions { get; set; }

        public FindChoicesOptions RecognizerOptions { get; set; }

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
            var culture = turnContext.Activity.Locale ?? DefaultLocale;
            if (string.IsNullOrEmpty(culture) || !DefaultChoiceOptions.ContainsKey(culture))
            {
                culture = English;
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
                opt.Locale = activity.Locale ?? opt.Locale ?? DefaultLocale ?? English;
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
