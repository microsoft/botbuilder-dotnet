// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A delegate definition of a Waterfall step. This is implemented by application code.
    /// </summary>
    /// <param name="stepContext">The WaterfallStepContext for this waterfall dialog.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> of <see cref="DialogTurnResult"/> representing the asynchronous operation.</returns>
    public delegate Task<DialogTurnResult> WaterfallStep(WaterfallStepContext stepContext, CancellationToken cancellationToken);
}
