// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Builder.Prompts.Results;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Choice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Prompts
{
    /// <summary>
    /// ConfirmPrompt recognizes confrimation expressions as bool
    /// </summary>
    public class ConfirmPrompt
    {
        private readonly IModel model;
        private List<Choice> ChoicesList => ChoiceFactory.ToChoicesList(this.Choices);

        private static readonly Dictionary<string, Tuple<Choice, Choice>> DefaultConfirmOptions = new Dictionary<string, Tuple<Choice, Choice>>()
        {
            { Spanish, new Tuple<Choice,Choice>( new Choice { Value = "Sí" }, new Choice { Value = "No" } ) },
            { Dutch, new Tuple<Choice,Choice>( new Choice { Value = "Ja" }, new Choice { Value = "Niet" } ) },
            { English, new Tuple<Choice,Choice>( new Choice { Value = "Yes" }, new Choice { Value = "No" } ) },
            { French, new Tuple<Choice,Choice>( new Choice { Value = "Oui" }, new Choice { Value = "Non" } ) },
            { German, new Tuple<Choice,Choice>( new Choice { Value = "Ja" }, new Choice { Value = "Nein" } ) },
            { Japanese, new Tuple<Choice,Choice>( new Choice { Value = "はい" }, new Choice { Value = "いいえ" } ) },
            { Portuguese, new Tuple<Choice,Choice>( new Choice { Value = "Sim" }, new Choice { Value = "Não" } ) },
            { Chinese, new Tuple<Choice,Choice>( new Choice { Value = "是的" }, new Choice { Value = "不" } ) }
        };

        private static readonly Dictionary<string, ChoiceFactoryOptions> DefaultInlineChoiceOptions = new Dictionary<string, ChoiceFactoryOptions>()
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

        public ListStyle Style { get; set; }
        public PromptValidator<ConfirmResult> Validator { get; set; }
        public string Culture { get; set; }
        public Tuple<Choice, Choice> Choices { get; set; }
        public ChoiceFactoryOptions ChoiceOptions { get; set; }
        public Dictionary<string, Tuple<Choice, Choice>> ConfirmOptions { get; set; }
        public Dictionary<string, ChoiceFactoryOptions> InlineChoiceOptions { get; set; }

        public ConfirmPrompt(string culture, PromptValidator<ConfirmResult> validator = null, Dictionary<string, Tuple<Choice, Choice>> confirmOptions = null, Dictionary<string, ChoiceFactoryOptions> inlineChoiceOptions = null, ListStyle listStyle = ListStyle.Auto)
        {
            model = new ChoiceRecognizer(culture).GetBooleanModel(culture);
            Style = listStyle;
            Validator = validator;
            Culture = culture;
            ConfirmOptions = confirmOptions != null ? confirmOptions : DefaultConfirmOptions;
            InlineChoiceOptions = inlineChoiceOptions != null ? inlineChoiceOptions : DefaultInlineChoiceOptions;
            Choices = ConfirmOptions.ContainsKey(culture) ? ConfirmOptions[culture] : ConfirmOptions[English];
            ChoiceOptions = InlineChoiceOptions.ContainsKey(culture) ? InlineChoiceOptions[culture] : InlineChoiceOptions[English];
        }

        public Task Prompt(ITurnContext context, string prompt = null, string speak = null)
        {
            BotAssert.ContextNotNull(context);
            return Prompt(context, ChoicesList, prompt, speak);
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
                    msg = ChoiceFactory.Inline(ChoicesList, prompt, speak, ChoiceOptions);
                    break;

                case ListStyle.List:
                    msg = ChoiceFactory.List(ChoicesList, prompt, speak, ChoiceOptions);
                    break;

                case ListStyle.SuggestedAction:
                    msg = ChoiceFactory.SuggestedAction(ChoicesList, prompt, speak);
                    break;

                case ListStyle.None:
                    msg = Activity.CreateMessageActivity();
                    msg.Text = prompt;
                    msg.Speak = speak;
                    break;

                default:
                    msg = ChoiceFactory.ForChannel(context, ChoicesList, prompt, speak, ChoiceOptions);
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