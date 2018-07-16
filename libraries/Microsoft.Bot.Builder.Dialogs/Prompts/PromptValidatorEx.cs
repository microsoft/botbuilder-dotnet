// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines a delegate for adding custom validation to a prompt object.
    /// </summary>
    /// <seealso cref="BasePromptInternal{T}"/>
    /// <seealso cref="PromptResult"/>
    public static class PromptValidatorEx
    {
        /// <summary>
        /// Validates a recognized value.
        /// and customize the reply sent to the user when their response is invalid.
        /// </summary>
        /// <typeparam name="T">Type of value to be recognized and passed to the validator as input.</typeparam>
        /// <param name="context">The context for the current turn.</param>
        /// <param name="toValidate">The value recognized.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>You can add custom validation when you create the prompt object.
        /// You can use the <paramref name="context"/> to send a validation success or failure
        /// message to the user.</remarks>
        public delegate Task PromptValidator<T>(ITurnContext context, T toValidate)
            where T : PromptResult;
    }
}
