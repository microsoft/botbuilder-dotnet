// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class ConcurrentStreamTests
    {
        [Fact]
        public async Task ConsumeInSmallerChunks()
        {
            await ProducerConsumerMultithreadedTest(1024, 100, 1024, 50);
        }

        [Fact]
        public async Task ConsumeInLargerChunks()
        {
            await ProducerConsumerMultithreadedTest(1024, 50, 1024, 100);
        }

        [Fact]
        public async Task ConsumeLessThanProduced()
        {
            await ProducerConsumerMultithreadedTest(1024, 50, 500, 100);
        }

        [Fact]
        public async Task ConsumeInOneChunks()
        {
            await ProducerConsumerMultithreadedTest(1024, 100, 1024, 1);
        }

        [Fact]
        public async Task ProduceInOneChunks()
        {
            await ProducerConsumerMultithreadedTest(1024, 1, 1024, 50);
        }

        [Fact]
        public async Task CanReadLess()
        {
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[producerBuffer.Length];
            const int expectedReadCount = 50;
            var readCount = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);

                readCount = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
            }

            Assert.Equal(expectedReadCount, readCount);
            Assert.Equal(producerBuffer.Take(expectedReadCount).ToArray(), consumerBuffer.Take(expectedReadCount).ToArray());
        }

        [Fact]
        public async Task CanReadExact()
        {
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[producerBuffer.Length];
            var expectedReadCount = producerBuffer.Length;
            var readCount = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);

                readCount = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
            }

            Assert.Equal(expectedReadCount, readCount);
            Assert.Equal(producerBuffer.Take(expectedReadCount).ToArray(), consumerBuffer.Take(expectedReadCount).ToArray());
        }

        [Fact]
        public async Task CanReadMore_GetLess()
        {
            const int expectedReadCount = 200;
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[expectedReadCount];
            var readCount = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);

                readCount = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
            }

            Assert.Equal(100, readCount);
            Assert.Equal(producerBuffer.Take(readCount).ToArray(), consumerBuffer.Take(readCount).ToArray());
        }

        [Fact]
        public async Task CanReadMore_GetLess_ThenMore_GivesFirstBuffer()
        {
            const int expectedReadCount = 200;
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[expectedReadCount];
            var readCount1 = 0;
            var readCount2 = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                // write 200
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);

                // ask for less
                readCount1 = await stream.ReadAsync(consumerBuffer, 0, 50);

                // ask for more than what should be left in 1 buffer
                readCount2 = await stream.ReadAsync(consumerBuffer, 50, 150);
            }

            Assert.Equal(50, readCount1);
            Assert.Equal(50, readCount2);
        }

        [Fact]
        public async Task CanRead_AfterMultipleWrites()
        {
            const int expectedReadCount = 200;
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[expectedReadCount];
            var readCount = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);

                readCount = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
            }

            // only get 1 buffer
            Assert.Equal(100, readCount);
            Assert.Equal(producerBuffer.Take(readCount).ToArray(), consumerBuffer.Take(readCount).ToArray());
        }

        [Fact]
        public async Task CanReadTwice_AfterMultipleWrites()
        {
            const int expectedReadCount = 200;
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[expectedReadCount];
            var readCount1 = 0;
            var readCount2 = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);

                readCount1 = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
                readCount2 = await stream.ReadAsync(consumerBuffer, readCount1, expectedReadCount);
            }

            // only get 1 buffer
            Assert.Equal(100, readCount1);
            Assert.Equal(100, readCount2);
            Assert.Equal(producerBuffer, consumerBuffer.Take(100).ToArray());
            Assert.Equal(producerBuffer, consumerBuffer.Skip(100).ToArray());
        }

        [Fact]
        public void CanRead_IsTrue()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.True(stream.CanRead);
            }
        }

        [Fact]
        public void CanWrite_IsTrue()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.True(stream.CanWrite);
            }
        }

        [Fact]
        public void CanSeek_IsFalse()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.False(stream.CanSeek);
            }
        }

        [Fact]
        public void PositionSetter_Throws()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.Throws<NotSupportedException>(() =>
                {
                    stream.Position = 10;
                });
            }
        }

        [Fact]
        public void SetLength_Throws()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.Throws<NotSupportedException>(() =>
                {
                    stream.SetLength(100);
                });
            }
        }

        [Fact]
        public void Seek_Throws()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.Throws<NotSupportedException>(() =>
                {
                    stream.Seek(100, SeekOrigin.Begin);
                });
            }
        }

        [Fact]
        public async Task DoneProducing_Empty_WillCauseZeroRead()
        {
            const int expectedReadCount = 200;
            var consumerBuffer = new byte[expectedReadCount];
            var readCount = 0;

            using (var stream = new PayloadStream(null))
            {
                stream.DoneProducing();

                readCount = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
            }

            Assert.Equal(0, readCount);
        }

        [Fact]
        public async Task DoneProducing_Data_WillCauseZeroRead()
        {
            const int expectedReadCount = 100;
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[expectedReadCount];
            var readCount1 = 0;
            var readCount2 = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);
                stream.DoneProducing();

                readCount1 = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
                readCount2 = await stream.ReadAsync(consumerBuffer, readCount1, expectedReadCount);
            }

            Assert.Equal(100, readCount1);
            Assert.Equal(0, readCount2);
        }

        [Fact]
        public async Task DoneProducing_Data_WillCauseZeroRead_And_End()
        {
            const int expectedReadCount = 100;
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[expectedReadCount];
            var readCount = 0;

            var streamManager = new StreamManager(e => { });
            var id = Guid.NewGuid();

            var assembler = streamManager.GetPayloadAssembler(id);
            assembler.ContentLength = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            var stream = new PayloadStream(assembler);

            await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);
            stream.DoneProducing();

            readCount = stream.Read(consumerBuffer, 0, expectedReadCount);
            Assert.Equal(100, readCount);

            readCount = stream.Read(consumerBuffer, readCount, expectedReadCount);
            Assert.Equal(0, readCount);
        }

        private async Task ProducerConsumerMultithreadedTest(
            int producerTotalCount,
            int producerChunkCount,
            int consumerTotalCount,
            int consumerChunkCount)
        {
            var producerBuffer = new byte[producerTotalCount];
            var consumerBuffer = new byte[consumerTotalCount];

            var random = new Random();
            random.NextBytes(producerBuffer);

            var producerPosition = 0;
            var consumerPosition = 0;

            using (var ct = new CancellationTokenSource())
            {
                using (var s = new PayloadStream(null))
                {
                    Func<Task> reader = async () =>
                    {
                        while (consumerPosition < consumerBuffer.Length)
                        {
                            var readCount = Math.Min(consumerChunkCount, consumerBuffer.Length - consumerPosition);

                            var bytesRead = await s.ReadAsync(consumerBuffer, consumerPosition, readCount, ct.Token);

                            if (bytesRead == 0)
                            {
                                break;
                            }

                            consumerPosition += bytesRead;
                        }
                    };

                    Func<Task> writer = async () =>
                    {
                        while (producerPosition < producerBuffer.Length)
                        {
                            var writeCount = Math.Min(producerChunkCount, producerBuffer.Length - producerPosition);

                            await s.WriteAsync(producerBuffer, producerPosition, writeCount, ct.Token);

                            producerPosition += writeCount;

                            await Task.Yield();
                        }
                    };

                    var readTask = reader();
                    var writeTask = writer();
                    await Task.WhenAll(readTask, writeTask);
                }
            }

            Assert.Equal(producerTotalCount, producerPosition);
            var consumableCount = Math.Min(producerTotalCount, consumerTotalCount);
            Assert.Equal(consumableCount, consumerPosition);
            Assert.Equal(producerBuffer.Take(consumableCount).ToArray(), consumerBuffer.Take(consumableCount).ToArray());
        }
    }
}
