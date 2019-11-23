// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Format to output text in.
    /// </summary>
    public enum TextOutputFormat
    {
        /// <summary>
        /// No formatting.
        /// </summary>
        None,

        /// <summary>
        /// Trim leading, trailing spaces.
        /// </summary>
        Trim,

        /// <summary>
        /// All lower case.
        /// </summary>
        Lowercase,

        /// <summary>
        /// All upper case.
        /// </summary>
        UpperCase
    }

    /// <summary>
    /// Declarative text input to gather text data from users.
    /// </summary>
    public class TextInput : InputDialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.TextInput";

        public TextInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        [JsonProperty("outputFormat")]
        public TextOutputFormat OutputFormat { get; set; } = TextOutputFormat.None;

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            var input = dc.GetState().GetValue<string>(VALUE_PROPERTY);

            switch (this.OutputFormat)
            {
                case TextOutputFormat.Trim:
                    input = input.Trim();
                    break;
                case TextOutputFormat.Lowercase:
                    input = input.ToLower();
                    break;
                case TextOutputFormat.UpperCase:
                    input = input.ToUpper();
                    break;
            }

            dc.GetState().SetValue(VALUE_PROPERTY, input);
            return input.Length > 0 ? Task.FromResult(InputState.Valid) : Task.FromResult(InputState.Unrecognized);
        }
    }
}
