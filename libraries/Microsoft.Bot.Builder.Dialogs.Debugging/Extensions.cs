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
    public static partial class Extensions
    {
        public static async Task<Releaser> WithWaitAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new Releaser(semaphore);
        }

        public struct Releaser : IDisposable
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
