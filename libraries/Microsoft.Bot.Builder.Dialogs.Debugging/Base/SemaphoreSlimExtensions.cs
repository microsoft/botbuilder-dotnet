// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Base
{
    /// <summary>
    /// Extension method implementing await for <see cref="SemaphoreSlim"/>.
    /// </summary>
    internal static class SemaphoreSlimExtensions
    {
        public static async Task<Releaser> WithWaitAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new Releaser(semaphore);
        }
    }
}
