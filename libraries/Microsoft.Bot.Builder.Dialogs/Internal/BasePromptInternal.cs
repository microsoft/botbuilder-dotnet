// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Represents a user prompt utility class, that includes an optional input validator.
    /// This class is abstract.
    /// </summary>
    /// <typeparam name="T">The the result type for the recognition output.</typeparam>
    /// <seealso cref="PromptValidator{InT}"/>
    /// <seealso cref="PromptResult"/>
    internal abstract class BasePromptInternal<T>
        where T : PromptResult
    {
        private readonly PromptValidator<T> _customValidator = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePromptInternal{T}"/> class.
        /// </summary>
        /// <param name="validator">The input validator for the prompt object.</param>
        public BasePromptInternal(PromptValidator<T> validator = null)
        {
            _customValidator = validator;
        }

        /// <summary>
        /// When overridden in a derived class, recognizes the user input.
        /// </summary>
        /// <param name="context">The context for the current turn.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this when you expect that the incoming activity for this
        /// turn contains the user input to recognize. If recognition succeeds, call
        /// the prompt object's <see cref="ValidateAsync(ITurnContext, T)"/> method.
        /// <para>If the recognition object includes a <c>Value</c> property and recognition succeeds,
        /// set this property to the value recognized.</para>
        /// <para>If recognition fails, return a <see cref="PromptResult"/> with
        /// its <see cref="PromptStatus"/> set to <see cref="PromptStatus.NotRecognized"/> and
        /// its <c>Value</c> property (if it has one) set to <c>null</c>.</para>
        /// </remarks>
        public abstract Task<T> RecognizeAsync(ITurnContext context);

        /// <summary>
        /// Validates a recognized value.
        /// </summary>
        /// <param name="context">The context for the current turn.</param>
        /// <param name="value">The recognized value.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the prompt object defined an input validator, runs it using the
        /// recognized value; otherwise, sets the <see cref="PromptResult.Status"/> of
        /// the prompt result to <see cref="PromptStatus.Recognized"/>.</remarks>
        protected virtual Task ValidateAsync(ITurnContext context, T value)
        {
            // Validation passed. Return the validated text.
            if (_customValidator != null)
            {
                return _customValidator(context, value);
            }

            value.Status = PromptStatus.Recognized;
            return Task.CompletedTask;
        }
    }
}
