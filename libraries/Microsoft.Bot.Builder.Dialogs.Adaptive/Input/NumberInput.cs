// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions.Properties;
using Microsoft.Recognizers.Text.Number;
using Newtonsoft.Json;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public class NumberInput : InputDialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.NumberInput";

        public NumberInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        [JsonProperty("defaultLocale")]
        public StringExpression DefaultLocale { get; set; } = null;

        [JsonProperty("outputFormat")]
        public NumberExpression OutputFormat { get; set; }

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            var dcState = dc.GetState();
            var input = dcState.GetValue<object>(VALUE_PROPERTY);

            var culture = GetCulture(dc);
            var results = NumberRecognizer.RecognizeNumber(input.ToString(), culture);
            if (results.Count > 0)
            {
                // Try to parse value based on type
                var text = results[0].Resolution["value"].ToString();

                if (int.TryParse(text, out var intValue))
                {
                    input = intValue;
                }
                else
                {
                    if (float.TryParse(text, out var value))
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

            dcState.SetValue(VALUE_PROPERTY, input);

            if (OutputFormat != null)
            {
                var (outputValue, error) = this.OutputFormat.TryGetValue(dcState);
                if (error == null)
                {
                    dcState.SetValue(VALUE_PROPERTY, outputValue);
                }
                else
                {
                    throw new Exception($"In TextInput, OutputFormat Expression evaluation resulted in an error. Expression: {this.OutputFormat}. Error: {error}");
                }
            }

            return Task.FromResult(InputState.Valid);
        }

        private string GetCulture(DialogContext dc)
        {
            if (!string.IsNullOrEmpty(dc.Context.Activity.Locale))
            {
                return dc.Context.Activity.Locale;
            }

            if (this.DefaultLocale != null)
            {
                var dcState = dc.GetState();

                return this.DefaultLocale.GetValue(dcState);
            }

            return English;
        }
    }
}
