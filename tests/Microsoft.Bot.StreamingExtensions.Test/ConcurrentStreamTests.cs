// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests
{
    [TestClass]
    public class ConcurrentStreamTests
    {
        [TestMethod]
        public async Task ConsumeInSmallerChunks()
        {
            await ProducerConsumerMultithreadedTest(1024, 100, 1024, 50);
        }

        [TestMethod]
        public async Task ConsumeInLargerChunks()
        {
            await ProducerConsumerMultithreadedTest(1024, 50, 1024, 100);
        }

        [TestMethod]
        public async Task ConsumeLessThanProduced()
        {
            await ProducerConsumerMultithreadedTest(1024, 50, 500, 100);
        }

        [TestMethod]
        public async Task ConsumeInOneChunks()
        {
            await ProducerConsumerMultithreadedTest(1024, 100, 1024, 1);
        }

        [TestMethod]
        public async Task ProduceInOneChunks()
        {
            await ProducerConsumerMultithreadedTest(1024, 1, 1024, 50);
        }

        [TestMethod]
        public async Task CanReadLess()
        {
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[producerBuffer.Length];
            int expectedReadCount = 50;
            int readCount = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);

                readCount = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
            }

            Assert.AreEqual(expectedReadCount, readCount);
            CollectionAssert.AreEquivalent(producerBuffer.Take(expectedReadCount).ToArray(), consumerBuffer.Take(expectedReadCount).ToArray());
        }

        [TestMethod]
        public async Task CanReadExact()
        {
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[producerBuffer.Length];
            int expectedReadCount = producerBuffer.Length;
            int readCount = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);

                readCount = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
            }

            Assert.AreEqual(expectedReadCount, readCount);
            CollectionAssert.AreEquivalent(producerBuffer.Take(expectedReadCount).ToArray(), consumerBuffer.Take(expectedReadCount).ToArray());
        }

        [TestMethod]
        public async Task CanReadMore_GetLess()
        {
            int expectedReadCount = 200;
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[expectedReadCount];
            int readCount = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);

                readCount = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
            }

            Assert.AreEqual(100, readCount);
            CollectionAssert.AreEquivalent(producerBuffer.Take(readCount).ToArray(), consumerBuffer.Take(readCount).ToArray());
        }

        [TestMethod]
        public async Task CanReadMore_GetLess_ThenMore_GivesFirstBuffer()
        {
            int expectedReadCount = 200;
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[expectedReadCount];
            int readCount1 = 0;
            int readCount2 = 0;

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

            Assert.AreEqual(50, readCount1);
            Assert.AreEqual(50, readCount2);
        }

        [TestMethod]
        public async Task CanRead_AfterMultipleWrites()
        {
            int expectedReadCount = 200;
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[expectedReadCount];
            int readCount = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);

                readCount = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
            }

            // only get 1 buffer
            Assert.AreEqual(100, readCount);
            CollectionAssert.AreEquivalent(producerBuffer.Take(readCount).ToArray(), consumerBuffer.Take(readCount).ToArray());
        }

        [TestMethod]
        public async Task CanReadTwice_AfterMultipleWrites()
        {
            int expectedReadCount = 200;
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[expectedReadCount];
            int readCount1 = 0;
            int readCount2 = 0;

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
            Assert.AreEqual(100, readCount1);
            Assert.AreEqual(100, readCount2);
            CollectionAssert.AreEquivalent(producerBuffer, consumerBuffer.Take(100).ToArray());
            CollectionAssert.AreEquivalent(producerBuffer, consumerBuffer.Skip(100).ToArray());
        }

        [TestMethod]
        public void CanRead_IsTrue()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.IsTrue(stream.CanRead);
            }
        }

        [TestMethod]
        public void CanWrite_IsTrue()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.IsTrue(stream.CanWrite);
            }
        }

        [TestMethod]
        public void CanSeek_IsFalse()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.IsFalse(stream.CanSeek);
            }
        }

        [TestMethod]
        public void PositionSetter_Throws()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.ThrowsException<NotSupportedException>(() =>
                {
                    stream.Position = 10;
                });
            }
        }

        [TestMethod]
        public void SetLength_Throws()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.ThrowsException<NotSupportedException>(() =>
                {
                    stream.SetLength(100);
                });
            }
        }

        [TestMethod]
        public void Seek_Throws()
        {
            using (var stream = new PayloadStream(null))
            {
                Assert.ThrowsException<NotSupportedException>(() =>
                {
                    stream.Seek(100, SeekOrigin.Begin);
                });
            }
        }

        [TestMethod]
        public async Task DoneProducing_Empty_WillCauseZeroRead()
        {
            int expectedReadCount = 200;
            var consumerBuffer = new byte[expectedReadCount];
            int readCount = 0;

            using (var stream = new PayloadStream(null))
            {
                stream.DoneProducing();

                readCount = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
            }

            Assert.AreEqual(0, readCount);
        }

        [TestMethod]
        public async Task DoneProducing_Data_WillCauseZeroRead()
        {
            int expectedReadCount = 100;
            var producerBuffer = new byte[100];
            var consumerBuffer = new byte[expectedReadCount];
            int readCount1 = 0;
            int readCount2 = 0;

            var random = new Random();
            random.NextBytes(producerBuffer);

            using (var stream = new PayloadStream(null))
            {
                await stream.WriteAsync(producerBuffer, 0, producerBuffer.Length);
                stream.DoneProducing();

                readCount1 = await stream.ReadAsync(consumerBuffer, 0, expectedReadCount);
                readCount2 = await stream.ReadAsync(consumerBuffer, readCount1, expectedReadCount);
            }

            Assert.AreEqual(100, readCount1);
            Assert.AreEqual(0, readCount2);
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

            int producerPosition = 0;
            int consumerPosition = 0;

            using (var ct = new CancellationTokenSource())
            {
                using (var s = new PayloadStream(null))
                {
                    Func<Task> reader = async () =>
                    {
                        while (consumerPosition < consumerBuffer.Length)
                        {
                            int readCount = Math.Min(consumerChunkCount, consumerBuffer.Length - consumerPosition);

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
                            int writeCount = Math.Min(producerChunkCount, producerBuffer.Length - producerPosition);

                            await s.WriteAsync(producerBuffer, producerPosition, writeCount, ct.Token);

                            producerPosition += writeCount;

                            await Task.Yield();
                        }
                    };

                    var readTask = reader();
                    var writetask = writer();
                    await Task.WhenAll(readTask, writetask);
                }
            }

            Assert.AreEqual(producerTotalCount, producerPosition);
            var consumableCount = Math.Min(producerTotalCount, consumerTotalCount);
            Assert.AreEqual(consumableCount, consumerPosition);
            CollectionAssert.AreEquivalent(producerBuffer.Take(consumableCount).ToArray(), consumerBuffer.Take(consumableCount).ToArray());
        }
    }
}
