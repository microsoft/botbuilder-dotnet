using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prompts
{
    /// <summary>
    /// Defines a delegate for adding custom validation to a prompt object.
    /// </summary>
    /// <seealso cref="BasePrompt{T}"/>
    /// <seealso cref="PromptResult"/>
    public static class PromptValidatorEx
    {
        /// <summary>
        /// Validates a recognized value.
        /// and customize the reply sent to the user when their response is invalid.
        /// </summary>
        /// <typeparam name="InT">Type of value to be recognized and passed to the validator as input.</typeparam>
        /// <param name="context">The context for the current turn.</param>
        /// <param name="toValidate">The value recognized.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>You can add custom validation when you create the prompt object.
        /// You can use the <paramref name="context"/> to send a validation success or failure
        /// message to the user.</remarks>
        public delegate Task PromptValidator<InT>(ITurnContext context, InT toValidate)
            where InT : PromptResult;
    }
}