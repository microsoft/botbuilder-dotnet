// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Declarative text input to gather text data from users.
    /// </summary>
    public class TextInput : InputDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TextInput";

        /// <summary>
        /// Initializes a new instance of the <see cref="TextInput"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public TextInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the expression to use to format the result.
        /// </summary>
        /// <remarks>The default output is a string, if this property is set then the output of the expression is the string returned by the dialog.</remarks>
        /// <value>an expression which resolves to a string.</value>
        [JsonProperty("outputFormat")]
        public StringExpression OutputFormat { get; set; }

        /// <summary>
        /// Called when input has been received.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>InputState which reflects whether input was recognized as valid or not.</returns>
        protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var input = dc.State.GetValue<string>(VALUE_PROPERTY);

            if (this.OutputFormat != null)
            {
                var (outputValue, error) = this.OutputFormat.TryGetValue(dc.State);
                if (error == null)
                {
                    if (!string.IsNullOrWhiteSpace(outputValue))
                    {
                        // if the result is null or empty string, ignore it.
                        input = outputValue.ToString();
                    }
                }
                else
                {
                    throw new InvalidOperationException($"In TextInput, OutputFormat Expression evaluation resulted in an error. Expression: {OutputFormat.ToString()}. Error: {error}");
                }
            }

            dc.State.SetValue(VALUE_PROPERTY, input);
            return input.Length > 0 ? Task.FromResult(InputState.Valid) : Task.FromResult(InputState.Unrecognized);
        }
    }
}
