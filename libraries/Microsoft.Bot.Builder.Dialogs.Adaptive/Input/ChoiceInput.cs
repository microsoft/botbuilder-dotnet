// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public class ChoiceInputOptions : InputDialogOptions
    {
        public List<Choice> Choices { get; set; }
    }

    public enum ChoiceOutputFormat
    {
        /// <summary>
        /// Return the value of the choice
        /// </summary>
        Value,

        /// <summary>
        /// return the index of the choice
        /// </summary>
        Index
    }

    /// <summary>
    /// Declarative text input to gather choices from users
    /// </summary>
    public class ChoiceInput : InputDialog
    {
        private static readonly Dictionary<string, ChoiceFactoryOptions> DefaultChoiceOptions = new Dictionary<string, ChoiceFactoryOptions>(StringComparer.OrdinalIgnoreCase)
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
        /// List of choices to present to user
        /// </summary>
        public List<Choice> Choices { get; set; }

        /// <summary>
        /// Expression collection of choices to present o user
        /// </summary>
        public string ChoicesProperty { get; set; }

        /// <summary>
        /// ListStyle to use to render the choices
        /// </summary>
        public ListStyle Style { get; set; } = ListStyle.Auto;

        /// <summary>
        /// DefaultLocale
        /// </summary>
        public string DefaultLocale { get; set; } = null;

        /// <summary>
        /// Control the format of the response (value or the index of the choice)
        /// </summary>
        public ChoiceOutputFormat OutputFormat { get; set; } = ChoiceOutputFormat.Value;

        /// <summary>
        /// ChoiceOptions controls display options for customizing language
        /// </summary>
        public ChoiceFactoryOptions ChoiceOptions { get; set; } = null;

        /// <summary>
        /// Customize how to use the choices to recognize the response from the user
        /// </summary>
        public FindChoicesOptions RecognizerOptions { get; set; } = null;

        public ChoiceInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        protected override object OnInitializeOptions(DialogContext dc, object options)
        {
            var op = options as ChoiceInputOptions;
            if (op == null || op.Choices == null || op.Choices.Count == 0)
            {
                if (op == null)
                {
                    op = new ChoiceInputOptions();
                }

                var choices = GetChoices(dc);
                op.Choices = choices;
            }

            return base.OnInitializeOptions(dc, op);
        }

        public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            FoundChoice foundChoice = result as FoundChoice;
            if (foundChoice != null)
            {
                // return value insted of FoundChoice object
                return base.ResumeDialogAsync(dc, reason, foundChoice.Value, cancellationToken);
            }

            return base.ResumeDialogAsync(dc, reason, result, cancellationToken);
        }

        protected override string OnComputeId()
        {
            return $"ChoiceInput[{BindingPath()}]";
        }

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            var input = dc.State.GetValue<object>(INPUT_PROPERTY);
            var options = dc.State.GetValue<ChoiceInputOptions>(DialogContextState.DIALOG_OPTIONS);

            var choices = options.Choices;

            var result = new PromptRecognizerResult<FoundChoice>();
            if (dc.Context.Activity.Type == ActivityTypes.Message)
            {
                var opt = this.RecognizerOptions ?? new FindChoicesOptions();
                opt.Locale = GetCulture(dc);
                var results = ChoiceRecognizers.RecognizeChoices(input.ToString(), choices, opt);
                if (results == null || results.Count == 0)
                {
                    return Task.FromResult(InputState.Unrecognized);
                }

                var foundChoice = results[0].Resolution;
                switch (this.OutputFormat)
                {
                    case ChoiceOutputFormat.Value:
                    default:
                        dc.State.SetValue(INPUT_PROPERTY, foundChoice.Value);
                        break;
                    case ChoiceOutputFormat.Index:
                        dc.State.SetValue(INPUT_PROPERTY, foundChoice.Index);
                        break;
                }
            }

            return Task.FromResult(InputState.Valid);
        }

        protected override async Task<IActivity> OnRenderPrompt(DialogContext dc, InputState state)
        {
            var locale = GetCulture(dc);
            var prompt = await base.OnRenderPrompt(dc, state);
            var channelId = dc.Context.Activity.ChannelId;
            var choicePrompt = new ChoicePrompt(this.Id);
            var choiceOptions = this.ChoiceOptions ?? ChoiceInput.DefaultChoiceOptions[locale];

            var choices = GetChoices(dc);

            return this.AppendChoices(prompt.AsMessageActivity(), channelId, choices, this.Style, choiceOptions);
        }

        private string GetCulture(DialogContext dc)
        {
            if (!string.IsNullOrWhiteSpace(dc.Context.Activity.Locale))
            {
                return dc.Context.Activity.Locale;
            }

            if (!string.IsNullOrWhiteSpace(this.DefaultLocale))
            {
                return this.DefaultLocale;
            }

            return English;
        }

        private List<Choice> GetChoices(DialogContext dc)
        {
            var choices = this.Choices ?? new List<Choice>();

            if (!string.IsNullOrEmpty(this.ChoicesProperty))
            {
                var choicesMemory = dc.State.GetValue<object>(this.ChoicesProperty).ToString();

                try
                {
                    choices = JsonConvert.DeserializeObject<List<Choice>>(choicesMemory);
                }
                catch
                {

                }

                if (choices == null || choices.Count == 0)
                {
                    try
                    {
                        var strList = JsonConvert.DeserializeObject<List<string>>(choicesMemory);
                        choices = strList.Select(item => new Choice(item)).ToList();
                    }
                    catch
                    {

                    }
                }
            }

            return choices;
        }
    }
}
