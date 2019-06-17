// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
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

        public ChoiceInputOptions()
        {
        }
    }

    public enum ChoiceOutputFormat
    {
        Value,
        Index
    }

    /// <summary>
    /// Declarative text input to gather choices from users
    /// </summary>
    public class ChoiceInput : InputDialog
    {
        public List<Choice> Choices { get; set; }

        public string ChoicesProperty { get; set; }

        public ListStyle Style { get; set; } = ListStyle.Auto;

        public string DefaultLocale { get; set; } = null;

        public ChoiceOutputFormat OutputFormat { get; set; } = ChoiceOutputFormat.Value;

        public ChoiceInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        // Override the base method since we need to pass choices to the prompt options
        //protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    if (options is CancellationToken)
        //    {
        //        throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
        //    }

        //    // Check value in state and only call if missing or required by AlwaysPrompt
        //    var hasValue = Property == null ? false : dc.State.HasValue(Property);

        //    if (hasValue == false || AlwaysPrompt)
        //    {
        //        if (Prompt == null)
        //        {
        //            throw new ArgumentNullException(nameof(Activity));
        //        }

        //        var prompt = await Prompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);
        //        var retryPrompt = RetryPrompt == null ? prompt : await RetryPrompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);

        //        this.prompt.Style = this.Style;

        //        var choices = this.Choices ?? new List<Choice>();

        //        if (!string.IsNullOrEmpty(this.ChoicesProperty))
        //        {
        //            if (dc.State.TryGetValue<object>(this.ChoicesProperty, out var choiceValue))
        //            {
        //                try
        //                {
        //                    if (choiceValue is string)
        //                    {
        //                        choices = JsonConvert.DeserializeObject<List<Choice>>(choiceValue.ToString());
        //                    }
        //                    else if (choiceValue is List<Choice>)
        //                    {
        //                        choices = (List<Choice>)choiceValue;
        //                    }
        //                }
        //                catch
        //                {

        //                }
        //            }
        //        }

        //        return await dc.PromptAsync(this.prompt.Id, new PromptOptions() { Prompt = prompt, RetryPrompt = retryPrompt, Choices = choices}, cancellationToken).ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        //    }
        //}
        protected override object OnInitializeOptions(DialogContext dc, object options)
        {
            var op = options as ChoiceInputOptions;
            if (op == null || op.Choices == null || op.Choices.Count == 0)
            {
                if (this.Choices != null && this.Choices.Count > 0)
                {
                    if (op == null)
                    {
                        op = new ChoiceInputOptions();
                    }
                    op.Choices = this.Choices;
                }
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

        protected override Task<InputState> OnRecognizeInput(DialogContext dc, bool consultation)
        {
            var input = dc.State.GetValue<string>(INPUT_PROPERTY);
            var options = dc.State.GetValue<ChoiceInputOptions>(OPTIONS_PROPERTY);

            var choices = options.Choices;

            var result = new PromptRecognizerResult<FoundChoice>();
            if (dc.Context.Activity.Type == ActivityTypes.Message)
            {
                var activity = dc.Context.Activity;
                var utterance = activity.Text;
                var opt = new FindChoicesOptions();
                opt.Locale = English;
                var results = ChoiceRecognizers.RecognizeChoices(utterance, choices, opt);
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
            var locale = "en-us";
            if (!string.IsNullOrEmpty(dc.Context.Activity.Locale))
            {
                locale = dc.Context.Activity.Locale;
            }
            else if (this.DefaultLocale != null)
            {
                locale = this.DefaultLocale;
            }

            var prompt = await base.OnRenderPrompt(dc, state);
            var channelId = dc.Context.Activity.ChannelId;
            var choicePrompt = new ChoicePrompt();
            return (IActivity)choicePrompt.AppendChoices(prompt.AsMessageActivity(), channelId, this.Choices, this.Style);
        }
    }
}
