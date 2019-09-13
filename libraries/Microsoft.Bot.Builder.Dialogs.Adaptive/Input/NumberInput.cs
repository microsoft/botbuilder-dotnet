// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Recognizers.Text.Number;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// What format to output the number in.
    /// </summary>
    public enum NumberOutputFormat
    {
        /// <summary>
        /// Floating point.
        /// </summary>
        Float,

        /// <summary>
        /// Long.
        /// </summary>
        Integer
    }

    public class NumberInput : InputDialog
    {
        public NumberInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        public string DefaultLocale { get; set; } = null;

        public NumberOutputFormat OutputFormat { get; set; } = NumberOutputFormat.Float;

        protected override string OnComputeId()
        {
            return $"NumberInput[{BindingPath()}]";
        }

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            var input = dc.State.GetValue<object>(INPUT_PROPERTY);

            var culture = GetCulture(dc);
            var results = NumberRecognizer.RecognizeNumber(input.ToString(), culture);
            if (results.Count > 0)
            {
                // Try to parse value based on type
                var text = results[0].Resolution["value"].ToString();
                    
                if (float.TryParse(text, out var value))
                {
                    input = value;
                }
                else
                {
                    return Task.FromResult(InputState.Unrecognized);
                }
            }
            else
            {
                return Task.FromResult(InputState.Unrecognized);
            }

            switch (this.OutputFormat)
            {
                case NumberOutputFormat.Float:
                default:
                    dc.State.SetValue(INPUT_PROPERTY, input);
                    break;
                case NumberOutputFormat.Integer:
                    dc.State.SetValue(INPUT_PROPERTY, Math.Floor((float)input));
                    break;
            }

            return Task.FromResult(InputState.Valid);
        }

        private string GetCulture(DialogContext dc)
        {
            if (!string.IsNullOrEmpty(dc.Context.Activity.Locale))
            {
                return dc.Context.Activity.Locale;
            }

            if (!string.IsNullOrEmpty(this.DefaultLocale))
            {
                return this.DefaultLocale;
            }

            return English;
        }
    }
}
