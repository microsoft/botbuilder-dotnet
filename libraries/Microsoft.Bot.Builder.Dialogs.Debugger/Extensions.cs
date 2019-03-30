using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Debugger
{
    public static partial class Extensions
    {
        public struct Releaser : IDisposable
        {
            public SemaphoreSlim Semaphore { get; }
            public Releaser(SemaphoreSlim semaphore)
            {
                Semaphore = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
            }
            public void Dispose()
            {
                Semaphore.Release();
            }
        }

        public static async Task<Releaser> WithWaitAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new Releaser(semaphore);
        }
    }
}
