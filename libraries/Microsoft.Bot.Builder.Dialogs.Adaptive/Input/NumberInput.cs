// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Generic declarative number input for gathering number information from users
    /// </summary>
    /// <typeparam name="TNumber"></typeparam>
    public class NumberInput<TNumber> : InputWrapper<NumberPrompt<TNumber>, TNumber> where TNumber : struct, IComparable<TNumber>
    {
        /// <summary>
        /// Minimum value expected for number
        /// </summary>
        public TNumber MinValue { get; set; }

        /// <summary>
        /// Maximum value expected for number
        /// </summary>
        public TNumber MaxValue { get; set; }

        public NumberInput()
        {
        }

        protected override NumberPrompt<TNumber> CreatePrompt()
        {
            // We override the default constructor behavior from base class to add custom validation around min and max values.
            return new NumberPrompt<TNumber>(null, new PromptValidator<TNumber>(async (promptContext, cancel) =>
            {
                var result = (IComparable<TNumber>)promptContext.Recognized.Value;

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
