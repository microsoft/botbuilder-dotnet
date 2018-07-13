// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Represents a user prompt class for text input.
    /// </summary>
    /// <remarks>The <see cref="Recognize(ITurnContext)"/> method passes any
    /// non-whitespace string to the custom validator, if one was provided.
    /// To change this behavior, derive from this class and add your own custom
    /// validation behavior.
    /// <para>For simple validation changes, specify a <see cref="PromptValidator{InT}"/>
    /// in the constructor. If the standard validation passes, the custom
    /// validator is called on the recognized value.</para>
    /// </remarks>
    internal class TextPromptInternal : BasePromptInternal<TextResult>
    {
        /// <summary>
        /// Creates a <see cref="TextPromptInternal"/> object.
        /// </summary>
        /// <param name="validator">The input validator for the prompt object.</param>
        /// <remarks><paramref name="validator"/> is called only if the
        /// <see cref="Recognize(ITurnContext)"/> method recognizes a value.
        /// </remarks>
        public TextPromptInternal(PromptValidator<TextResult> validator = null)
            : base(validator)
        {
        }

        /// <summary>
        /// Recognizes and validates the user input.
        /// </summary>
        /// <param name="context">The context for the current turn.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this when you expect that the incoming activity for this
        /// turn contains the user input to recognize.
        /// If recognition succeeds, the <see cref="TextResult.Value"/> property of the
        /// result contains the value recognized.
        /// <para>If recognition fails, returns a <see cref="TextResult"/> with
        /// its <see cref="PromptStatus"/> set to <see cref="PromptStatus.NotRecognized"/> and
        /// its <see cref="TextResult.Value"/> set to <c>null</c>.</para>
        /// </remarks>
        public override async Task<TextResult> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            var message = context.Activity as MessageActivity;
            var textResult = new TextResult();

            if (message.Text != null)
            {
                textResult.Status = PromptStatus.Recognized;
                textResult.Value = message.Text;
                textResult.Text = message.Text;
                await Validate(context, textResult);
            }
            return textResult;
        }
    }
}
