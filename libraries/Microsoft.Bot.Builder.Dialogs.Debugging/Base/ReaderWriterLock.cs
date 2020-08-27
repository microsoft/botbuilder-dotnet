// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Base
{
    internal sealed class ReaderWriterLock : IDisposable
    {
        private readonly SemaphoreSlim _reader;
        private readonly SemaphoreSlim _writer;
        private int _readers;

        public ReaderWriterLock(bool writer = false)
        {
            _reader = new SemaphoreSlim(1, 1);
            _writer = new SemaphoreSlim(writer ? 0 : 1, 1);
        }

        public void Dispose()
        {
            _reader.Dispose();
            _writer.Dispose();
        }

        public async Task<bool> TryEnterReadAsync(CancellationToken cancellationToken)
        {
            using (await _reader.WithWaitAsync(cancellationToken).ConfigureAwait(false))
            {
                var acquired = true;

                if (_readers == 0)
                {
                    acquired = await _writer.WaitAsync(TimeSpan.Zero, cancellationToken).ConfigureAwait(false);
                }

                if (acquired)
                {
                    ++_readers;
                }

                return acquired;
            }
        }

        public async Task ExitReadAsync(CancellationToken token)
        {
            using (await _reader.WithWaitAsync(token).ConfigureAwait(false))
            {
                --_readers;
                if (_readers == 0)
                {
                    _writer.Release();
                }
            }
        }

        public async Task EnterWriteAsync(CancellationToken token)
        {
            await _writer.WaitAsync(token).ConfigureAwait(false);
        }

        public void ExitWrite()
        {
            _writer.Release();
        }
    }
}
