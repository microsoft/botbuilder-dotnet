// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
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
        public string OutputFormat { get; set; }

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            var input = dc.GetState().GetValue<string>(VALUE_PROPERTY);

            if (!string.IsNullOrEmpty(OutputFormat))
            {
                var outputExpression = new ExpressionEngine().Parse(OutputFormat);
                var (outputValue, error) = outputExpression.TryEvaluate(dc.GetState());
                if (error == null)
                {
                    input = outputValue.ToString();
                }
                else
                {
                    throw new Exception($"In TextInput, OutputFormat Expression evaluation resulted in an error. Expression: {outputExpression.ToString()}. Error: {error}");
                }
            }

            dc.GetState().SetValue(VALUE_PROPERTY, input);
            return input.Length > 0 ? Task.FromResult(InputState.Valid) : Task.FromResult(InputState.Unrecognized);
        }
    }
}
