// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Choices;
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
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets list of choices to present to user.
        /// </summary>
        /// <value>
        /// ChoiceSet or expression which evalutes to a ChoiceSet.
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
        /// ChoiceOptions or expression which evluates to ChoiceOptions.
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
        public ObjectExpression<FindChoicesOptions> RecognizerOptions { get; set; } = null;

        /// <summary>
        /// Replaces the result with the FoundChoice value if possible, then proceedes to <see cref="Dialog.ResumeDialogAsync(DialogContext, DialogReason, object, CancellationToken)"/>.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="reason">Reason why the dialog resumed.</param>
        /// <param name="result">Optional, value returned from the dialog that was called. The type
        /// of the value returned is dependent on the child dialog.</param>
        /// <param name="cancellationToken">Optional, a <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

                var (choices, error) = this.Choices.TryGetValue(dc.State);
                if (error != null)
                {
                    throw new InvalidOperationException(error);
                }

                op.Choices = choices;
            }

            return base.OnInitializeOptions(dc, op);
        }

        /// <summary>
        /// Called when input has been received, recognices choice.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>InputState which reflects whether input was recognized as valid or not.</returns>
        protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var input = dc.State.GetValue<object>(VALUE_PROPERTY);
            var options = dc.State.GetValue<ChoiceInputOptions>(ThisPath.Options);

            var choices = options.Choices;

            if (dc.Context.Activity.Type == ActivityTypes.Message)
            {
                var opt = this.RecognizerOptions?.GetValue(dc.State) ?? new FindChoicesOptions();
                opt.Locale = GetCulture(dc);
                var results = ChoiceRecognizers.RecognizeChoices(input.ToString(), choices, opt);
                if (results == null || results.Count == 0)
                {
                    return Task.FromResult(InputState.Unrecognized);
                }

                var foundChoice = results[0].Resolution;
                switch (this.OutputFormat.GetValue(dc.State))
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
        protected override async Task<IActivity> OnRenderPromptAsync(DialogContext dc, InputState state, CancellationToken cancellationToken = default(CancellationToken))
        {
            var locale = GetCulture(dc);
            var prompt = await base.OnRenderPromptAsync(dc, state, cancellationToken).ConfigureAwait(false);
            var channelId = dc.Context.Activity.ChannelId;
            var choicePrompt = new ChoicePrompt(this.Id);
            var choiceOptions = this.ChoiceOptions?.GetValue(dc.State) ?? ChoiceInput.DefaultChoiceOptions[locale];

            var (choices, error) = this.Choices.TryGetValue(dc.State);
            if (error != null)
            {
                throw new InvalidOperationException(error);
            }

            return this.AppendChoices(prompt.AsMessageActivity(), channelId, choices, this.Style.GetValue(dc.State), choiceOptions);
        }

        private string GetCulture(DialogContext dc)
        {
            if (!string.IsNullOrWhiteSpace(dc.Context.Activity.Locale))
            {
                return dc.Context.Activity.Locale;
            }

            if (this.DefaultLocale != null)
            {
                return this.DefaultLocale.GetValue(dc.State);
            }

            return English;
        }
    }
}
