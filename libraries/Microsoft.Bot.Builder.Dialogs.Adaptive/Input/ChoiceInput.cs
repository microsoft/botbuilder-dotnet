﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Prompts;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Response format definition.
    /// </summary>
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
    /// ChoiceInput - Declarative input to gather choices from user.
    /// </summary>
    public class ChoiceInput : InputDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ChoiceInput";

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
        /// Initializes a new instance of the <see cref="ChoiceInput"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public ChoiceInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets list of choices to present to user.
        /// </summary>
        /// <value>
        /// ChoiceSet or expression which evaluates to a ChoiceSet.
        /// </value>
        [JsonProperty("choices")]
        public ObjectExpression<ChoiceSet> Choices { get; set; }

        /// <summary>
        /// Gets or sets ListStyle to use to render the choices.
        /// </summary>
        /// <value>
        /// ListStyle or expression which evaluates to ListStyle.
        /// </value>
        [JsonProperty("style")]
        public EnumExpression<ListStyle> Style { get; set; } = ListStyle.Auto;

        /// <summary>
        /// Gets or sets the DefaultLocale to use to parse confirmation choices if there is not one passed by the caller.
        /// </summary>
        /// <value>
        /// string or expression which evaluates to a string with locale.
        /// </value>
        [JsonProperty("defaultLocale")]
        public StringExpression DefaultLocale { get; set; }

        /// <summary>
        /// Gets or sets the format of the response (value or the index of the choice).
        /// </summary>
        /// <value>
        /// ChoiceOutputFormat or Expression which evaluates to ChoiceOutputFormat enumerated value.
        /// </value>
        [JsonProperty("outputFormat")]
        public EnumExpression<ChoiceOutputFormat> OutputFormat { get; set; } = ChoiceOutputFormat.Value;

        /// <summary>
        /// Gets or sets choiceOptions controls display options for customizing language.
        /// </summary>
        /// <value>
        /// ChoiceOptions or expression which evaluates to ChoiceOptions.
        /// </value>
        [JsonProperty("choiceOptions")]
        public ObjectExpression<ChoiceFactoryOptions> ChoiceOptions { get; set; }

        /// <summary>
        /// Gets or sets how to recognize choices in the response.
        /// </summary>
        /// <value>
        /// FindChoicesOptions or expression which evaluates to FindChoicesOptions.
        /// </value>
        [JsonProperty("recognizerOptions")]
        public ObjectExpression<FindChoicesOptions> RecognizerOptions { get; set; }

        /// <summary>
        /// Replaces the result with the FoundChoice value if possible, then proceeds to <see cref="Dialog.ResumeDialogAsync(DialogContext, DialogReason, object, CancellationToken)"/>.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="reason">Reason why the dialog resumed.</param>
        /// <param name="result">Optional, value returned from the dialog that was called. The type
        /// of the value returned is dependent on the child dialog.</param>
        /// <param name="cancellationToken">Optional, a <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is FoundChoice foundChoice)
            {
                // return value instead of FoundChoice object
                return base.ResumeDialogAsync(dc, reason, foundChoice.Value, cancellationToken);
            }

            return base.ResumeDialogAsync(dc, reason, result, cancellationToken);
        }

        /// <inheritdoc/>
        internal override void TrackGeneratorResultEvent(DialogContext dc, ITemplate<Activity> activityTemplate, IMessageActivity msg)
        {
            var options = dc.State.GetValue<ChoiceInputOptions>(ThisPath.Options);
            var serializationSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, MaxDepth = null };
            var properties = new Dictionary<string, string>()
            {
                { "template", JsonConvert.SerializeObject(activityTemplate, new JsonSerializerSettings { MaxDepth = null }) },
                { "result", msg == null ? string.Empty : JsonConvert.SerializeObject(msg, serializationSettings) },
                { "choices", options.Choices == null ? string.Empty : JsonConvert.SerializeObject(options.Choices, serializationSettings) },
                { "context", TelemetryLoggerConstants.InputDialogResultEvent }
            };
            TelemetryClient.TrackEvent(TelemetryLoggerConstants.GeneratorResultEvent, properties);
        }

        /// <summary>
        /// Method which processes options.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <returns>modified options.</returns>
        protected override object OnInitializeOptions(DialogContext dc, object options)
        {
            var op = options as ChoiceInputOptions;
            if (op == null || op.Choices == null || op.Choices.Count == 0)
            {
                if (op == null)
                {
                    op = new ChoiceInputOptions();
                }

                op.Choices = GetChoiceSetAsync(dc).GetAwaiter().GetResult();
            }

            return base.OnInitializeOptions(dc, op);
        }

        /// <summary>
        /// Called when input has been received, recognizes choice.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>InputState which reflects whether input was recognized as valid or not.</returns>
        protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            var input = dc.State.GetValue<object>(VALUE_PROPERTY);
            var options = dc.State.GetValue<ChoiceInputOptions>(ThisPath.Options);

            var choices = options.Choices;

            if (dc.Context.Activity.Type == ActivityTypes.Message)
            {
                var opt = RecognizerOptions?.GetValue(dc.State) ?? new FindChoicesOptions();
                opt.Locale = DetermineCulture(dc, opt);
                var results = ChoiceRecognizers.RecognizeChoices(input.ToString(), choices, opt);
                if (results == null || results.Count == 0)
                {
                    return Task.FromResult(InputState.Unrecognized);
                }

                var foundChoice = results[0].Resolution;
                switch (OutputFormat.GetValue(dc.State))
                {
                    case ChoiceOutputFormat.Value:
                    default:
                        dc.State.SetValue(VALUE_PROPERTY, foundChoice.Value);
                        break;
                    case ChoiceOutputFormat.Index:
                        dc.State.SetValue(VALUE_PROPERTY, foundChoice.Index);
                        break;
                }
            }

            return Task.FromResult(InputState.Valid);
        }

        /// <summary>
        /// Method which renders the prompt to the user given the current input state.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="state">Dialog <see cref="InputState"/>.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Activity to send to the user.</returns>
        protected override async Task<IActivity> OnRenderPromptAsync(DialogContext dc, InputState state, CancellationToken cancellationToken = default)
        {
            var locale = DetermineCulture(dc);
            var prompt = await base.OnRenderPromptAsync(dc, state, cancellationToken).ConfigureAwait(false);
            var channelId = dc.Context.Activity.ChannelId;
            var opts = await GetChoiceOptionsAsync(dc, locale).ConfigureAwait(false);
            var choiceOptions = opts ?? DefaultChoiceOptions[locale];
            var options = dc.State.GetValue<ChoiceInputOptions>(ThisPath.Options);

            return AppendChoices(prompt.AsMessageActivity(), channelId, options.Choices, Style.GetValue(dc.State), choiceOptions);
        }

        private async Task<ChoiceSet> GetChoiceSetAsync(DialogContext dc)
        {
            if (Choices.ExpressionText != null && Choices.ExpressionText.TrimStart().StartsWith("${", StringComparison.InvariantCultureIgnoreCase))
            {
                // use ITemplate<ChocieSet> to bind (aka LG)
                return await new ChoiceSet(Choices.ExpressionText).BindAsync(dc, dc.State).ConfigureAwait(false);
            }
            else
            {
                // use Expression to bind
                return Choices.TryGetValue(dc.State).Value;
            }
        }

        private async Task<ChoiceFactoryOptions> GetChoiceOptionsAsync(DialogContext dc, string locale)
        {
            if (ChoiceOptions != null)
            {
                if (ChoiceOptions.ExpressionText != null && ChoiceOptions.ExpressionText.TrimStart().StartsWith("${", StringComparison.InvariantCultureIgnoreCase))
                {
                    // use ITemplate<ChoiceOptionsSet> to bind (aka LG)
                    return await new ChoiceOptionsSet(ChoiceOptions.ExpressionText).BindAsync(dc).ConfigureAwait(false);
                }
                else
                {
                    // use Expression to bind
                    return ChoiceOptions.TryGetValue(dc.State).Value;
                }
            }
            else
            {
                return DefaultChoiceOptions[locale];
            }
        }

        private string DetermineCulture(DialogContext dc, FindChoicesOptions opt = null)
        {
            // Note: opt.Locale and Default locale will be considered for deprecation as part of 4.13.
            var candidateLocale = dc.GetLocale() ?? opt?.Locale ?? DefaultLocale?.GetValue(dc.State);
            var culture = PromptCultureModels.MapToNearestLanguage(candidateLocale);

            if (string.IsNullOrEmpty(culture) || !DefaultChoiceOptions.ContainsKey(culture))
            {
                culture = English;
            }

            return culture;
        }
    }
}
