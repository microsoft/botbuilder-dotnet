// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Signature of a handler that can be passed to a prompt to provide additional validation logic
    /// or to customize the reply sent to the user when their response is invalid.
    /// </summary>
    /// <typeparam name="R">Type of value that will recognized and passed to the validator as input.</typeparam>
    /// <typeparam name="O">Type of value that will recognized and passed to the validator as input.</typeparam>
    /// <param name="context">PromptValidator.context Context for the current turn of conversation.</param>
    /// <param name="value">PromptValidator.value The value that was recognized or `undefined` if not recognized.</param>
    public delegate Task<O> PromptValidator<R, O>(TurnContext context, R value);
}
