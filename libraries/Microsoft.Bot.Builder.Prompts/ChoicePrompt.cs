// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    ///<summary>
    /// Controls the way that choices for a `ChoicePrompt` or yes/no options for a `ConfirmPrompt` are
    /// presented to a user.
    ///</summary>
    public enum ListStyle
    {
        ///<summary>
        /// Don't include any choices for prompt.
        /// </summary>
        None,

        ///<summary>
        /// Automatically select the appropriate style for the current channel.
        /// </summary>
        Auto,

        ///<summary>
        /// Add choices to prompt as an inline list.
        ///</summary>
        Inline,

        ///<summary>
        /// Add choices to prompt as a numbered list.
        ///</summary>
        List,

        ///<summary>
        /// Add choices to prompt as suggested actions.
        ///</summary>
        SuggestedAction
    };

    /// <summary>
    /// Represents recognition result for the prompt.
    /// </summary>
    public class ChoiceResult : PromptResult
    {
        /// <summary>
        /// Creates a <see cref="ChoiceResult"/> object.
        /// </summary>
        public ChoiceResult() { }

        /// <summary>
        /// The value recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public FoundChoice Value { get; set; }
    }

    public class ChoicePrompt
    {
        public ChoicePrompt(string culture, PromptValidator<ChoiceResult> validator = null)
        {
            Style = ListStyle.Auto;
            Validator = validator;
            Culture = culture;
        }

        public ListStyle Style { get; set; }
        public PromptValidator<ChoiceResult> Validator { get; set; }
        public string Culture { get; set; }
        public ChoiceFactoryOptions ChoiceOptions { get; set; }
        public FindChoicesOptions RecognizerOptions { get; set; }

        public Task Prompt(ITurnContext context, List<string> choices, string prompt = null, string speak = null)
        {
            BotAssert.ContextNotNull(context);
            if (choices == null)
                throw new ArgumentNullException(nameof(choices));

            return Prompt(context, ChoiceFactory.ToChoices(choices), prompt, speak);
        }

        public async Task Prompt(ITurnContext context, List<Choice> choices, string prompt = null, string speak = null)
        {
            BotAssert.ContextNotNull(context);
            if (choices == null)
                throw new ArgumentNullException(nameof(choices));

            IMessageActivity msg;

            switch (Style)
            {
                case ListStyle.Inline:
                    msg = ChoiceFactory.Inline(choices, prompt, speak, ChoiceOptions);
                    break;
                case ListStyle.List:
                    msg = ChoiceFactory.List(choices, prompt, speak, ChoiceOptions);
                    break;
                case ListStyle.SuggestedAction:
                    msg = ChoiceFactory.SuggestedAction(choices, prompt, speak);
                    break;
                case ListStyle.None:
                    msg = Activity.CreateMessageActivity();
                    msg.Text = prompt;
                    msg.Speak = speak;
                    break;
                case ListStyle.Auto:
                default:
                    msg = ChoiceFactory.ForChannel(context, choices, prompt, speak, ChoiceOptions);
                    break;
            }

            msg.InputHint = InputHints.ExpectingInput;
            await context.SendActivity(msg);
        }

        public async Task Prompt(ITurnContext context, IMessageActivity prompt = null, string speak = null)
        {
            BotAssert.ContextNotNull(context);

            if (prompt != null)
            {
                prompt.Speak = speak ?? prompt.Speak;
                await context.SendActivity(prompt);
            }
        }

        public Task<ChoiceResult> Recognize(ITurnContext context, List<string> choices)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");
            if (choices == null)
                throw new ArgumentNullException(nameof(choices));

            return Recognize(context, ChoiceFactory.ToChoices(choices));
        }

        public async Task<ChoiceResult> Recognize(ITurnContext context, List<Choice> choices)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");
            if (choices == null)
                throw new ArgumentNullException(nameof(choices));

            var request = context.Activity;
            var utterance = request.Text;
            var options = RecognizerOptions ?? new FindChoicesOptions();
            options.Locale = request.Locale ?? options.Locale ?? Culture ?? Recognizers.Text.Culture.English;
            var results = ChoiceRecognizers.RecognizeChoices(utterance, choices, options);
            if (results != null && results.Count > 0)
            {
                var value = results[0].Resolution;
                var result = new ChoiceResult { Status = PromptStatus.Recognized, Value = value };
                if (Validator != null)
                {
                    await Validator(context, result);
                }
                return result;
            }
            else
            {
                return new ChoiceResult { Status = PromptStatus.NotRecognized };
            }
        }
    }
}
