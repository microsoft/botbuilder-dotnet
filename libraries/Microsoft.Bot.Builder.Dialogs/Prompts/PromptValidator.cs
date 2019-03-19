// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// The delegate definition for custom prompt validators. Implement this function to add custom validation to a prompt.
    /// </summary>
    /// <typeparam name="T">The type the associated <see cref="Prompt{T}"/> prompts for.</typeparam>
    /// <param name="promptContext">The prompt validation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> of bool representing the asynchronous operation indicating validation success or failure.</returns>
    public delegate Task<bool> PromptValidator<T>(PromptValidatorContext<T> promptContext, CancellationToken cancellationToken);
}
