// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.Choice;
using Newtonsoft.Json;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Declarative input control that will gather yes/no confirmation input.
    /// </summary>
    public class ConfirmInput : InputDialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.ConfirmInput";

        private static readonly Dictionary<string, (Choice, Choice, ChoiceFactoryOptions)> ChoiceDefaults = new Dictionary<string, (Choice, Choice, ChoiceFactoryOptions)>(StringComparer.OrdinalIgnoreCase)
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

        public ConfirmInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        [JsonProperty("defaultLocale")]
        public StringExpression DefaultLocale { get; set; }

        [JsonProperty("style")]
        public EnumExpression<ListStyle> Style { get; set; } = ListStyle.Auto;

        [JsonProperty("choiceOptions")]
        public ObjectExpression<ChoiceFactoryOptions> ChoiceOptions { get; set; }

        [JsonProperty("confirmChoices")]
        public ArrayExpression<Choice> ConfirmChoices { get; set; }

        [JsonProperty("outputFormat")]
        public StringExpression OutputFormat { get; set; }

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            var input = dc.State.GetValue<object>(VALUE_PROPERTY);
            if (dc.Context.Activity.Type == ActivityTypes.Message)
            {
                // Recognize utterance
                var culture = GetCulture(dc);
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
                                throw new Exception($"OutputFormat Expression evaluation resulted in an error. Expression: {OutputFormat.ToString()}. Error: {error}");
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
                    var defaults = ChoiceDefaults[culture];
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

        protected override async Task<IActivity> OnRenderPrompt(DialogContext dc, InputState state)
        {
            // Format prompt to send
            var channelId = dc.Context.Activity.ChannelId;
            var culture = GetCulture(dc);
            var defaults = ChoiceDefaults[culture];
            var choiceOptions = ChoiceOptions?.GetValue(dc.State) ?? defaults.Item3;
            var confirmChoices = ConfirmChoices?.GetValue(dc.State) ?? new List<Choice>() { defaults.Item1, defaults.Item2 };

            var prompt = await base.OnRenderPrompt(dc, state);
            var (style, error) = this.Style.TryGetValue(dc.State);
            return this.AppendChoices(prompt.AsMessageActivity(), channelId, confirmChoices, style, choiceOptions);
        }

        private string GetCulture(DialogContext dc)
        {
            if (!string.IsNullOrEmpty(dc.Context.Activity.Locale))
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
