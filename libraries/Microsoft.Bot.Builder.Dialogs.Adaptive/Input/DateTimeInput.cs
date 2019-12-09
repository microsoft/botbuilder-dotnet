// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions;
using Microsoft.Recognizers.Text.DateTime;
using Newtonsoft.Json;
using static Microsoft.Recognizers.Text.Culture;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public class DateTimeInput : InputDialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.DateTimeInput";

        public DateTimeInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        [JsonProperty("defaultLocale")]
        public string DefaultLocale { get; set; } = null;

        [JsonProperty("outputFormat")]
        public string OutputFormat { get; set; }

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            var input = dc.GetState().GetValue<object>(VALUE_PROPERTY);

            var culture = GetCulture(dc);
            var results = DateTimeRecognizer.RecognizeDateTime(input.ToString(), culture);
            if (results.Count > 0)
            {
                // Return list of resolutions from first match
                var result = new List<DateTimeResolution>();
                var values = (List<Dictionary<string, string>>)results[0].Resolution["values"];
                foreach (var value in values)
                {
                    result.Add(ReadResolution(value));
                }

                dc.GetState().SetValue(VALUE_PROPERTY, result);
                if (!string.IsNullOrEmpty(OutputFormat))
                {
                    var outputExpression = new ExpressionEngine().Parse(OutputFormat);
                    var (outputValue, error) = outputExpression.TryEvaluate(dc.GetState());
                    if (error == null)
                    {
                        dc.GetState().SetValue(VALUE_PROPERTY, outputValue);
                    }
                    else
                    {
                        throw new Exception($"OutputFormat Expression evaluation resulted in an error. Expression: {outputExpression.ToString()}. Error: {error}");
                    }
                }
            }
            else
            {
                return Task.FromResult(InputState.Unrecognized);
            }

            return Task.FromResult(InputState.Valid);
        }

        private DateTimeResolution ReadResolution(IDictionary<string, string> resolution)
        {
            var result = new DateTimeResolution();

            if (resolution.TryGetValue("timex", out var timex))
            {
                result.Timex = timex;
            }

            if (resolution.TryGetValue("value", out var value))
            {
                result.Value = value;
            }

            if (resolution.TryGetValue("start", out var start))
            {
                result.Start = start;
            }

            if (resolution.TryGetValue("end", out var end))
            {
                result.End = end;
            }

            return result;
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
