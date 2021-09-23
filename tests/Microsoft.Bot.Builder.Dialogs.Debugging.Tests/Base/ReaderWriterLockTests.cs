// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ReaderWriterLock = Microsoft.Bot.Builder.Dialogs.Debugging.Base.ReaderWriterLock;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.Base
{
    public sealed class ReaderWriterLockTests
    {
        [Fact]
        public void ReadWriterLock_DefaultWriter()
        {
            Assert.NotNull(new ReaderWriterLock());
        }

        [Fact]
        public void ReadWriterLock_WriterTrue()
        {
            Assert.NotNull(new ReaderWriterLock(true));
        }

        [Fact]
        public async Task ReadWriterLock_Dispose()
        {
            var readerWriter = new ReaderWriterLock(true);

            readerWriter.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await readerWriter.TryEnterReadAsync(default);
            });
        }

        [Fact]
        public async Task ReadWriterLock_TryEnterReadAsync_AcquiredReader()
        {
            var readerWriter = new ReaderWriterLock();

            var acquired = await readerWriter.TryEnterReadAsync(default);

            Assert.True(acquired);
        }

        [Fact]
        public async Task ReadWriterLock_TryEnterReadAsync_NoAcquiredReader()
        {
            var readerWriter = new ReaderWriterLock(true);
            
            var acquired = await readerWriter.TryEnterReadAsync(default);

            Assert.False(acquired);
        }

        [Fact]
        public async Task ReadWriterLock_ExitReadAsync()
        {
            var readerWriter = new ReaderWriterLock();
            var token = new CancellationToken();

            var acquired = await readerWriter.TryEnterReadAsync(token);
            Assert.True(acquired);

            await readerWriter.ExitReadAsync(token);

            var secondAcquired = await readerWriter.TryEnterReadAsync(token);
            Assert.True(secondAcquired);
        }

        [Fact]
        public void ReadWriterLock_ExitWrite()
        {
            var readerWriter = new ReaderWriterLock(true);

            // Release the semaphore once.
            readerWriter.ExitWrite();

            // When trying to release it again it should throw exception
            Assert.Throws<SemaphoreFullException>(() =>
            {
                readerWriter.ExitWrite();
            });
        }
    }
}
