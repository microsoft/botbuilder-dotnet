// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Choice;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Prompts
{
    public class ConfirmResult : PromptResult
    {
        /// <summary>
        /// The input bool recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public bool Confirmation
        {
            get { return GetProperty<bool>(nameof(Confirmation)); }
            set { this[nameof(Confirmation)] = value; }
        }

        /// <summary>
        /// The input text recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public string Text
        {
            get { return GetProperty<string>(nameof(Text)); }
            set { this[nameof(Text)] = value; }
        }
    }

    /// <summary>
    /// ConfirmPrompt recognizes confrimation expressions as bool 
    /// </summary>
    public class ConfirmPrompt
    {
        private readonly IModel model;
        public ListStyle Style { get; set; }
        public PromptValidator<ConfirmResult> Validator { get; set; }
        public string Culture { get; set; }
        public List<string> Choices { get; set; }
        public ChoiceFactoryOptions ChoiceOptions { get; set; }

        private static readonly Dictionary<string, List<string>> ConfirmOptions = new Dictionary<string, List<string>>()
        {
            { Spanish, new List<string>{ "Sí", "No"} },
            { Dutch, new List<string>{ "Ja", "Niet"} },
            { English, new List<string>{ "Yes", "No"} },
            { French, new List<string>{ "Oui", "Non"} },
            { German, new List<string>{ "Ja", "Nein"} },
            { Japanese, new List<string>{ "はい", "いいえ" } },
            { Portuguese, new List<string>{ "Sim", "Não" } },
            { Chinese, new List<string>{ "是的", "不" } }
        };

        private static readonly Dictionary<string, ChoiceFactoryOptions> InlineChoiceOptions = new Dictionary<string, ChoiceFactoryOptions>()
        {
            { Spanish, new ChoiceFactoryOptions{ InlineSeparator = ", ", InlineOr = " o ", InlineOrMore = ", o ", IncludeNumbers = true} },
            { Dutch, new ChoiceFactoryOptions{ InlineSeparator = ", ", InlineOr = " of ", InlineOrMore = ", of ", IncludeNumbers = true} },
            { English, new ChoiceFactoryOptions{ InlineSeparator = ", ", InlineOr = " or ", InlineOrMore = ", or ", IncludeNumbers = true} },
            { French, new ChoiceFactoryOptions{ InlineSeparator = ", ", InlineOr = " ou ", InlineOrMore = ", ou ", IncludeNumbers = true} },
            { German, new ChoiceFactoryOptions{ InlineSeparator = ", ", InlineOr = " oder ", InlineOrMore = ", oder ", IncludeNumbers = true} },
            { Japanese, new ChoiceFactoryOptions{ InlineSeparator = "、 ", InlineOr = " または ", InlineOrMore = "、 または ", IncludeNumbers = true} },
            { Portuguese, new ChoiceFactoryOptions{ InlineSeparator = ", ", InlineOr = " ou ", InlineOrMore = ", ou ", IncludeNumbers = true} },
            { Chinese, new ChoiceFactoryOptions{ InlineSeparator = "， ", InlineOr = " 要么 ", InlineOrMore = "， 要么 ", IncludeNumbers = true} }
        };

        public ConfirmPrompt(string culture, PromptValidator<ConfirmResult> validator = null, ListStyle listStyle = ListStyle.Auto)
        {
            model = new ChoiceRecognizer(culture).GetBooleanModel(culture);
            Style = listStyle;
            Validator = validator;
            Culture = culture;
            Choices = ConfirmOptions.ContainsKey(culture) ? ConfirmOptions[culture] : ConfirmOptions[English];
            ChoiceOptions = InlineChoiceOptions.ContainsKey(culture) ? InlineChoiceOptions[culture] : InlineChoiceOptions[English];
        }

        public Task Prompt(ITurnContext context, string prompt = null, string speak = null)
        {
            BotAssert.ContextNotNull(context);
            return Prompt(context, ChoiceFactory.ToChoices(Choices), prompt, speak);
        }

        public async Task Prompt(ITurnContext context, List<Choice> choices, string prompt = null, string speak = null)
        {
            BotAssert.ContextNotNull(context);
            if (Choices == null)
                throw new ArgumentNullException(nameof(choices));

            IMessageActivity msg;

            switch (Style)
            {
                case ListStyle.Inline:
                    msg = ChoiceFactory.Inline(Choices, prompt, speak, ChoiceOptions);
                    break;
                case ListStyle.List:
                    msg = ChoiceFactory.List(Choices, prompt, speak, ChoiceOptions);
                    break;
                case ListStyle.SuggestedAction:
                    msg = ChoiceFactory.SuggestedAction(Choices, prompt, speak);
                    break;
                case ListStyle.None:
                    msg = Activity.CreateMessageActivity();
                    msg.Text = prompt;
                    msg.Speak = speak;
                    break;
                default:
                    msg = ChoiceFactory.ForChannel(context, Choices, prompt, speak, ChoiceOptions);
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

        public async Task<ConfirmResult> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            IMessageActivity message = context.Activity.AsMessageActivity();
            var confirmResult = new ConfirmResult();
            var results = model.Parse(message.Text);
            if (results.Any())
            {
                var result = results.First();
                if (bool.TryParse(result.Resolution["value"].ToString(), out bool value))
                {
                    confirmResult.Status = PromptStatus.Recognized;
                    confirmResult.Confirmation = value;
                    confirmResult.Text = result.Text;
                    if (Validator != null)
                    {
                        await Validator(context, confirmResult);
                    }
                }
            }
            return confirmResult;
        }
    }
}