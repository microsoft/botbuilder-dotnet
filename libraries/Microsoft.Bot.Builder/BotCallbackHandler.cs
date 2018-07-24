// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// The callback delegate for application code.
    /// </summary>
    /// <param name="context">The turn context.</param>
    /// <param name="cancellationToken">The task cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public delegate Task BotCallbackHandler(ITurnContext context, CancellationToken cancellationToken);
}
