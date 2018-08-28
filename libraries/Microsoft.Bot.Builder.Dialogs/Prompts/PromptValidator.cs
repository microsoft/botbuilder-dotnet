// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public delegate Task PromptValidator<T>(ITurnContext turnContext, PromptValidatorContext<T> prompt, CancellationToken cancellationToken);
}
