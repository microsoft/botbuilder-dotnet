// Licensed under the MIT License.
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
using Microsoft.Recognizers.Text.Choice;
using Newtonsoft.Json;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Declarative input control that will gather yes/no confirmation input from a set of choices.
    /// </summary>
    public class ConfirmInput : InputDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ConfirmInput";

        private static readonly Dictionary<string, (Choice, Choice, ChoiceFactoryOptions)> DefaultChoiceOptions = new Dictionary<string, (Choice, Choice, ChoiceFactoryOptions)>(StringComparer.OrdinalIgnoreCase)
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
        /// Initializes a new instance of the <see cref="ConfirmInput"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public ConfirmInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the DefaultLocale to use to parse confirmation choices if there is not one passed by the caller.
        /// </summary>
        /// <value>
        /// The locale (en-us, nl-nl, etc) or expression which evaluates to locale.
        /// </value>
        [JsonProperty("defaultLocale")]
        public StringExpression DefaultLocale { get; set; }

        /// <summary>
        /// Gets or sets the ListType to use to prompt the user.
        /// </summary>
        /// <value>The listStyle or expression which evaluates to ListStyle.</value>
        [JsonProperty("style")]
        public EnumExpression<ListStyle> Style { get; set; } = ListStyle.Auto;

        /// <summary>
        /// Gets or sets the ChoiceOptions to use to format presenting confirmation choices to the user.
        /// </summary>
        /// <value>The listStyle or expression which evaluates to the ListStyle.</value>
        [JsonProperty("choiceOptions")]
        public ObjectExpression<ChoiceFactoryOptions> ChoiceOptions { get; set; }

        /// <summary>
        /// Gets or sets the Choices to present.
        /// </summary>
        /// <value>The choices or an expression which evaluates to the choices.</value>
        [JsonProperty("confirmChoices")]
        public ObjectExpression<ChoiceSet> ConfirmChoices { get; set; }

        /// <summary>
        /// Gets or sets the expression to use to format the result.
        /// </summary>
        /// <remarks>The default output is a bool, if this property is set then the output of the expression is the value returned by the dialog.</remarks>
        /// <value>an expression.</value>
        [JsonProperty("outputFormat")]
        public ValueExpression OutputFormat { get; set; }

        /// <summary>
        /// Called when input has been received.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>InputState which reflects whether input was recognized as valid or not.</returns>
        protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            var input = dc.State.GetValue<object>(VALUE_PROPERTY);
            if (dc.Context.Activity.Type == ActivityTypes.Message)
            {
                // Recognize utterance
                var culture = DetermineCulture(dc);
                var results = ChoiceRecognizer.RecognizeBoolean(input.ToString(), culture);
                if (results.Count > 0)
                {
                    var first = results[0];
                    if (bool.TryParse(first.Resolution["value"].ToString(), out var value))
                    {
                        dc.State.SetValue(VALUE_PROPERTY, value);
                        if (OutputFormat != null)
                        {
                            var (outputValue, error) = OutputFormat.TryGetValue(dc.State);
                            if (error == null)
                            {
                                dc.State.SetValue(VALUE_PROPERTY, outputValue);
                            }
                            else
                            {
                                throw new InvalidOperationException($"OutputFormat Expression evaluation resulted in an error. Expression: {OutputFormat}. Error: {error}");
                            }
                        }

                        return Task.FromResult(InputState.Valid);
                    }
                    else
                    {
                        return Task.FromResult(InputState.Unrecognized);
                    }
                }
                else
                {
                    // First check whether the prompt was sent to the user with numbers - if it was we should recognize numbers
                    var defaults = DefaultChoiceOptions[culture];
                    var choiceOptions = ChoiceOptions?.GetValue(dc.State) ?? defaults.Item3;

                    // This logic reflects the fact that IncludeNumbers is nullable and True is the default set in Inline style
                    if (!choiceOptions.IncludeNumbers.HasValue || choiceOptions.IncludeNumbers.Value)
                    {
                        // The text may be a number in which case we will interpret that as a choice.
                        var confirmChoices = ConfirmChoices?.GetValue(dc.State) ?? new List<Choice>() { defaults.Item1, defaults.Item2 };
                        var secondAttemptResults = ChoiceRecognizers.RecognizeChoices(input.ToString(), confirmChoices);
                        if (secondAttemptResults.Count > 0)
                        {
                            input = secondAttemptResults[0].Resolution.Index == 0;
                            dc.State.SetValue(VALUE_PROPERTY, input);
                        }
                        else
                        {
                            return Task.FromResult(InputState.Unrecognized);
                        }
                    }
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
            // Format prompt to send
            var channelId = dc.Context.Activity.ChannelId;
            var culture = DetermineCulture(dc);
            var defaults = DefaultChoiceOptions[culture];
            var choiceOptions = ChoiceOptions?.GetValue(dc.State) ?? defaults.Item3;
            var confirmChoices = ConfirmChoices?.GetValue(dc.State) ?? new List<Choice>() { defaults.Item1, defaults.Item2 };

            var prompt = await base.OnRenderPromptAsync(dc, state, cancellationToken).ConfigureAwait(false);
            var (style, _) = Style.TryGetValue(dc.State);
            return AppendChoices(prompt.AsMessageActivity(), channelId, confirmChoices, style, choiceOptions);
        }

        private string DetermineCulture(DialogContext dc)
        {
            // Note: Default locale will be considered for deprecation as part of 4.13.
            var candidateLocale = dc.GetLocale() ?? DefaultLocale?.GetValue(dc.State);
            var culture = PromptCultureModels.MapToNearestLanguage(candidateLocale);

            if (string.IsNullOrEmpty(culture) || !DefaultChoiceOptions.ContainsKey(culture))
            {
                culture = English;
            }

            return culture;
        }
    }
}
