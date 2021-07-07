// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Recognizers.Text.Number;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Input dialog for asking for numbers.
    /// </summary>
    public class NumberInput : InputDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.NumberInput";

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberInput"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public NumberInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the DefaultLocale to use to parse confirmation choices if there is not one passed by the caller.
        /// </summary>
        /// <value>
        /// string or expression which evaluates to a string with locale.
        /// </value>
        [JsonProperty("defaultLocale")]
        public StringExpression DefaultLocale { get; set; } = null;

        /// <summary>
        /// Gets or sets the format of the response (value or the index of the choice).
        /// </summary>
        /// <value>
        /// Expression which evaluates to a number.
        /// </value>
        [JsonProperty("outputFormat")]
        public NumberExpression OutputFormat { get; set; }

        /// <summary>
        /// Called when input has been received.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>InputState which reflects whether input was recognized as valid or not.</returns>
        protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var input = dc.State.GetValue<object>(VALUE_PROPERTY);

            var culture = GetCulture(dc);
            var results = NumberRecognizer.RecognizeNumber(input.ToString(), culture);
            if (results.Count > 0)
            {
                // Try to parse value based on type
                var text = results[0].Resolution["value"].ToString();

                if (long.TryParse(text, out var intValue))
                {
                    input = intValue;
                }
                else
                {
                    if (double.TryParse(text, out var value))
                    {
                        input = value;
                    }
                    else
                    {
                        return Task.FromResult(InputState.Unrecognized);
                    }
                }
            }
            else
            {
                return Task.FromResult(InputState.Unrecognized);
            }

            dc.State.SetValue(VALUE_PROPERTY, input);

            if (OutputFormat != null)
            {
                var (outputValue, error) = this.OutputFormat.TryGetValue(dc.State);
                if (error == null)
                {
                    dc.State.SetValue(VALUE_PROPERTY, outputValue);
                }
                else
                {
                    throw new InvalidOperationException($"In TextInput, OutputFormat Expression evaluation resulted in an error. Expression: {this.OutputFormat}. Error: {error}");
                }
            }

            return Task.FromResult(InputState.Valid);
        }

        private string GetCulture(DialogContext dc)
        {
            // Note: Default locale will be considered for deprecation as part of 4.13.
            return dc.GetLocale() ?? DefaultLocale?.GetValue(dc.State) ?? string.Empty;
        }
    }
}
