// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    /// <summary>
    /// Represents a user prompt utility class, that includes an optional input validator.
    /// This class is abstract.
    /// </summary>
    /// <typeparam name="T">The the result type for the recognition output.</typeparam>
    /// <seealso cref="PromptValidator{InT}"/>
    /// <seealso cref="PromptResult"/>
    public abstract class BasePrompt<T>
        where T : PromptResult
    {
        private readonly PromptValidator<T> _customValidator = null;

        /// <summary>
        /// Creates a <see cref="BasePrompt{T}"/> object.
        /// </summary>
        /// <param name="validator">The input validator for the prompt object.</param>
        public BasePrompt(PromptValidator<T> validator = null)
        {
            _customValidator = validator;
        }

        /// <summary>
        /// Sends a message to the user, using the context for the current turn.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="text">The text of the message to send.</param>
        /// <param name="speak">Optional, text to be spoken by your bot on a speech-enabled
        /// channel.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>The message is sent with the "expectingInput" input hint.
        /// <para>See the channel's documentation for limits imposed upon the contents of
        /// <paramref name="text"/>.</para>
        /// <para>To control various characteristics of your bot's speech such as voice,
        /// rate, volume, pronunciation, and pitch, specify <paramref name="speak"/> in
        /// Speech Synthesis Markup Language (SSML) format.</para>
        /// </remarks>
        public Task Prompt(ITurnContext context, string text, string speak = null)
        {
            IMessageActivity ma = Activity.CreateMessageActivity();
            ma.Text = !string.IsNullOrWhiteSpace(text) ? text : null;
            ma.Speak = !string.IsNullOrWhiteSpace(speak) ? speak : null;
            ma.InputHint = InputHints.ExpectingInput;
            return Prompt(context, ma);
        }

        /// <summary>
        /// Sends a message to the user, using the context for the current turn.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="activity">The message activity to send.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task Prompt(ITurnContext context, IMessageActivity activity)
        {
            await context.SendActivity(activity);
        }

        /// <summary>
        /// When overridden in a derived class, recognizes the user input.
        /// </summary>
        /// <param name="context">The context for the current turn.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this when you expect that the incoming activity for this
        /// turn contains the user input to recognize. If recognition succeeds, call
        /// the prompt object's <see cref="Validate(ITurnContext, T)"/> method.
        /// <para>If the recognition object includes a <c>Value</c> property and recognition succeeds,
        /// set this property to the value recognized.</para>
        /// <para>If recognition fails, return a <see cref="PromptResult"/> with
        /// its <see cref="PromptStatus"/> set to <see cref="PromptStatus.NotRecognized"/> and
        /// its <c>Value</c> property (if it has one) set to <c>null</c>.</para>
        /// </remarks>
        public abstract Task<T> Recognize(ITurnContext context);

        /// <summary>
        /// Validates a recognized value.
        /// </summary>
        /// <param name="context">The context for the current turn.</param>
        /// <param name="value">The recognized value.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the prompt object defined an input validator, runs it using the
        /// recognized value; otherwise, sets the <see cref="PromptResult.Status"/> of
        /// the prompt result to <see cref="PromptStatus.Recognized"/>.</remarks>
        protected virtual Task Validate(ITurnContext context, T value)
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