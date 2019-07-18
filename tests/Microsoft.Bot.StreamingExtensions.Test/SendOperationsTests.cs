// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.Bot.StreamingExtensions.PayloadTransport;
using Microsoft.Bot.StreamingExtensions.Transport;
using Microsoft.Bot.StreamingExtensions.UnitTests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.StreamingExtensions.UnitTests
{
    [TestClass]
    public class SendOperationsTests
    {
        [TestMethod]
        public async Task SendRequestAsync_WaitsTillAllDataSent()
        {
            var payLoadSender = new PayloadSender();
            var tcs = new TaskCompletionSource<bool>();
            DisconnectedEventHandler eventHandler = (sender, args) =>
            {
                tcs.TrySetException(new Exception(args.Reason));
            };
            payLoadSender.Disconnected += eventHandler;
            payLoadSender.Connect(GetMockedTransportSender(tcs, TransportConstants.MaxPayloadLength * 4));
            var sendOperations = new SendOperations(payLoadSender);
            try
            {
                using (var stream = GetMockedStream(TransportConstants.MaxPayloadLength * 4))
                {
                    var request = new StreamingRequest();
                    request.AddStream(new StreamContent(stream));
                    await sendOperations.SendRequestAsync(Guid.NewGuid(), request);
                }

                await tcs.Task;
            }
            finally
            {
                payLoadSender.Disconnected -= eventHandler;
            }
        }

        [TestMethod]
        public async Task RequestDisassembler_WithVariableStream_Sends()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);
            var ops = new SendOperations(sender);

            var request = StreamingRequest.CreatePost("/a/b");
            var stream = new PayloadStream(new PayloadStreamAssembler(null, Guid.NewGuid(), "blah", 100));
            stream.Write(new byte[100], 0, 100);
            request.AddStream(new StreamContent(stream));

            await ops.SendRequestAsync(Guid.NewGuid(), request);

            Assert.AreEqual(5, transport.Buffers.Count);
        }

        [TestMethod]
        public async Task RequestDisassembler_WithJsonStream_Sends()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);
            var ops = new SendOperations(sender);

            var request = StreamingRequest.CreatePost("/a/b");
            request.AddStream(new StringContent("abc", Encoding.ASCII));

            await ops.SendRequestAsync(Guid.NewGuid(), request);

            Assert.AreEqual(4, transport.Buffers.Count);
        }

        // Creates a stream that throws if read from after Disposal. Otherwise returns a buffer of byte data
        private Stream GetMockedStream(int length)
        {
            int read = 0;
            int isStreamDisposed = 1;
            var mockedStream = new Mock<Stream>(MockBehavior.Strict);
            mockedStream.SetupGet(a => a.Length).Returns(length);
            mockedStream.SetupGet(a => a.CanSeek).Returns(false);
            mockedStream
                .Setup(a => a.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns((byte[] buffer, int offset, int count, CancellationToken token) =>
                {
                    count = Math.Min(length - read, count);
                    if (count > 0)
                    {
                        if (isStreamDisposed == 0)
                        {
                            throw new ObjectDisposedException(nameof(Stream));
                        }

                        var data = Enumerable.Repeat((byte)10, count).ToArray();
                        Buffer.BlockCopy(data, 0, buffer, 0, count);
                        read += count;
                    }

                    return Task.FromResult(count);
                });
            mockedStream.As<IDisposable>().Setup(a => a.Dispose()).Callback(() => Interlocked.Exchange(ref isStreamDisposed, 0));
            return mockedStream.Object;
        }

        // Gets a transport sender that signals the tcs if it receives expected count of data.
        private ITransportSender GetMockedTransportSender(TaskCompletionSource<bool> tcs, int length)
        {
            var dataSent = 0;
            var transportSenderMock = new Mock<ITransportSender>(MockBehavior.Strict);
            transportSenderMock.SetupGet(a => a.IsConnected).Returns(true);
            transportSenderMock
                .Setup(a => a.SendAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((byte[] buffer, int offset, int count) =>
                {
                    dataSent += count;
                    if (dataSent >= length)
                    {
                        tcs.TrySetResult(true);
                    }

                    return Task.FromResult(count);
                });
            return transportSenderMock.Object;
        }
    }
}
