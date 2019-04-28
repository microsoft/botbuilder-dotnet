// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Generic declarative number input for gathering number information from users
    /// </summary>
    /// <typeparam name="float"></typeparam>
    public class NumberInput : InputWrapper<NumberPrompt<float>, float> 
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

        public NumberInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        protected override NumberPrompt<float> CreatePrompt()
        {
            // We override the default constructor behavior from base class to add custom validation around min and max values.
            return new NumberPrompt<float>(null, new PromptValidator<float>(async (promptContext, cancel) =>
            {
                if (!promptContext.Recognized.Succeeded)
                {
                    return false;
                }

                promptContext.Recognized.Value = (float)Math.Round(promptContext.Recognized.Value, Precision);
                var result = (IComparable<float>)promptContext.Recognized.Value;
                if (result.CompareTo(MinValue) < 0 || result.CompareTo(MaxValue) > 0)
                {
                    if (InvalidPrompt != null)
                    {
                        var invalid = await InvalidPrompt.BindToData(promptContext.Context, promptContext.State).ConfigureAwait(false);
                        if (invalid != null)
                        {
                            await promptContext.Context.SendActivityAsync(invalid).ConfigureAwait(false);
                        }

                    }

                    return false;
                }

                return true;
            }));
        }
    }
}
