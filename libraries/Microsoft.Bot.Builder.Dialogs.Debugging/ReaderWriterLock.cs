// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class ReaderWriterLock : IDisposable
    {
        private readonly SemaphoreSlim reader;
        private readonly SemaphoreSlim writer;
        private int readers = 0;

        public ReaderWriterLock(bool writer = false)
        {
            this.reader = new SemaphoreSlim(1, 1);
            this.writer = new SemaphoreSlim(writer ? 0 : 1, 1);
        }

        public void Dispose()
        {
            reader.Dispose();
            writer.Dispose();
        }

        public async Task<bool> TryEnterReadAsync(CancellationToken token)
        {
            using (await reader.WithWaitAsync(token).ConfigureAwait(false))
            {
                bool acquired = true;

                if (readers == 0)
                {
                    acquired = await writer.WaitAsync(TimeSpan.Zero).ConfigureAwait(false);
                }

                if (acquired)
                {
                    ++readers;
                }

                return acquired;
            }
        }

        public async Task ExitReadAsync(CancellationToken token)
        {
            using (await reader.WithWaitAsync(token).ConfigureAwait(false))
            {
                --readers;
                if (readers == 0)
                {
                    writer.Release();
                }
            }
        }

        public async Task EnterWriteAsync(CancellationToken token)
        {
            await writer.WaitAsync(token).ConfigureAwait(false);
        }

        public void ExitWrite()
        {
            writer.Release();
        }
    }
}
