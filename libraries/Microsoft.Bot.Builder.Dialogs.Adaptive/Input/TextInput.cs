// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public enum TextOutputFormat
    {
        None,
        Trim,
        Lowercase,
        UpperCase
    };

    /// <summary>
    /// Declarative text input to gather text data from users
    /// </summary>
    public class TextInput : InputDialog
    {
        public TextOutputFormat OutputFormat { get; set; } = TextOutputFormat.None;

        public TextInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.AllowInterruptions = AllowInterruptions.Always;
        }

        protected override string OnComputeId()
        {
            return $"TextInput[{BindingPath()}]";
        }

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            var input = dc.State.GetValue<string>(INPUT_PROPERTY);

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

            dc.State.SetValue(INPUT_PROPERTY, input);
            return input.Length > 0 ? Task.FromResult(InputState.Valid) : Task.FromResult(InputState.Unrecognized);
        }
    }
}
