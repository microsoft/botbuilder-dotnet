// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// Extension method implementing await for <see cref="SemaphoreSlim"/>.
    /// </summary>
    public static class SemaphoreSlimExtensions
    {
        public static async Task<Releaser> WithWaitAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new Releaser(semaphore);
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
#pragma warning disable CA1815 // Override equals and operator equals on value types (we don't seem to need this in code, excluding for now)
        public struct Releaser : IDisposable
#pragma warning restore CA1815 // Override equals and operator equals on value types
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public Releaser(SemaphoreSlim semaphore)
            {
                Semaphore = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
            }

            public SemaphoreSlim Semaphore { get; }

            public void Dispose()
            {
                Semaphore.Release();
            }
        }
    }
}
