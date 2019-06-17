// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.Number;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public enum NumberOutputFormat
    {
        Float,
        Integer
    }
    /// <summary>
    /// Generic declarative number input for gathering number information from users
    /// </summary>
    /// <typeparam name="float"></typeparam>
    public class NumberInput : InputDialog
    {
        /// <summary>
        /// Minimum value expected for number
        /// </summary>
        public float MinValue { get; set; } = float.MinValue;

        /// <summary>
        /// Maximum value expected for number
        /// </summary>
        public float MaxValue { get; set; } = float.MaxValue;

        /// <summary>
        /// Precision
        /// </summary>
        public int Precision { get; set; } = 0;

        public string DefaultLocale { get; set; } = null;

        public NumberOutputFormat OutputFormat { get; set; } = NumberOutputFormat.Float;

        public NumberInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        protected override Task<InputState> OnRecognizeInput(DialogContext dc, bool consultation)
        {
            var input = dc.State.GetValue<object>(INPUT_PROPERTY);

            var culture = dc.Context.Activity.Locale ?? DefaultLocale ?? English;
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
    }
}
