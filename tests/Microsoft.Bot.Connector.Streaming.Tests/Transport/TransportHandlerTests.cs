// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Tests.Features;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Xunit;
using RequestModel = Microsoft.Bot.Connector.Streaming.Payloads.RequestPayload;
using ResponseModel = Microsoft.Bot.Connector.Streaming.Payloads.ResponsePayload;

namespace Microsoft.Bot.Connector.Streaming.Tests
{
    public class TransportHandlerTests
    {
        public static IEnumerable<object[]> PipeToObserverData =>
            new List<object[]>()
            {
                new object[] { new List<(Header Header, byte[] Payload)>() },
                new object[] { GenerateHeaderPayloadData(1, 1) },
                new object[] { GenerateHeaderPayloadData(1, 1) },
                new object[] { GenerateHeaderPayloadData(10, 1) },
                new object[] { GenerateHeaderPayloadData(100, 1) },
                new object[] { GenerateHeaderPayloadData(1000, 1) },
                new object[] { GenerateHeaderPayloadData(10000, 1) },
                new object[] { GenerateHeaderPayloadData(100000, 1) },
                new object[] { GenerateHeaderPayloadData(100000, 2) },
                new object[] { GenerateHeaderPayloadData(1000, 20) },
                new object[] { GenerateHeaderPayloadData(1000, 200) },
            };

        public static IEnumerable<object[]> ErrorScenarioData =>
            new List<object[]>()
            {
                new object[] { GenerateHeaderPayloadData(1000, 2), true, false, false },
                new object[] { GenerateHeaderPayloadData(1000, 2), false, true, false },
                new object[] { GenerateHeaderPayloadData(1000, 2), false, false, true },
            };

        [Theory]
        [MemberData(nameof(PipeToObserverData))]
        public async Task TransportHandler_ReceiveFromPipe_IsSentToObserver(
            List<(Header Header, byte[] Payload)> transportData)
        {
            await RunTransportHandlerReceiveTestAsync(transportData, false, false, false);
        }

        [Theory]
        [MemberData(nameof(ErrorScenarioData))]
        public async Task TransportHandler_ReceiveFromPipe_ErrorScenarios(
            List<(Header Header, byte[] Payload)> transportData,
            bool cancelAfterFirst,
            bool cancelWithoutPayload,
            bool completeWithException)
        {
            await RunTransportHandlerReceiveTestAsync(transportData, cancelAfterFirst, cancelWithoutPayload, completeWithException);
        }

        [Fact]
        public void TransportHandler_NullObserver_Throws()
        {
            var pipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            var transportHandler = new TransportHandler(pipePair.Transport, NullLogger.Instance);
            Assert.Throws<ArgumentNullException>(() => transportHandler.Subscribe(null));
        }

        [Fact]
        public void TransportHandler_DoubleObserverRegistration_Throws()
        {
            var pipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            var transportHandler = new TransportHandler(pipePair.Transport, NullLogger.Instance);
            transportHandler.Subscribe(new TestTransportObserver());
            Assert.Throws<InvalidOperationException>(() => transportHandler.Subscribe(new TestTransportObserver()));
        }

        [Fact]
        public async Task TransportHandler_SendRequest_ThrowsOnNull()
        {
            var pipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            var transportHandler = new TransportHandler(pipePair.Transport, NullLogger.Instance);
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await transportHandler.SendRequestAsync(Guid.NewGuid(), null, CancellationToken.None));
        }

        [Fact]
        public async Task TransportHandler_SendRequest_TransportReceivesHeaderAndPayload()
        {
            var request = new RequestModel()
            {
                Verb = "GET",
                Path = "api/version",
                Streams = new List<StreamDescription>()
                {
                    new StreamDescription() { ContentType = "json", Id = Guid.NewGuid().ToString(), Length = 18 },
                    new StreamDescription() { ContentType = "text", Id = Guid.NewGuid().ToString(), Length = 24 }
                }
            };

            var pipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            var transportHandler = new TransportHandler(pipePair.Transport, NullLogger.Instance);

            var transport = pipePair.Application.Input;

            await transportHandler.SendRequestAsync(Guid.NewGuid(), request, CancellationToken.None);

            var result = await transport.ReadAsync();
            var buffer = result.Buffer;

            var headerBuffer = buffer.Slice(0, Math.Min(TransportConstants.MaxHeaderLength, buffer.Length));

            var header = HeaderSerializer.Deserialize(headerBuffer.ToArray(), 0, TransportConstants.MaxHeaderLength);

            buffer = buffer.Slice(TransportConstants.MaxHeaderLength);

            if (buffer.Length < header.PayloadLength)
            {
                transport.AdvanceTo(buffer.Start, buffer.End);

                result = await transport.ReadAsync();

                Assert.False(result.IsCanceled);

                buffer = result.Buffer;
            }

            var payload = buffer.Slice(buffer.Start, header.PayloadLength).ToArray();

            var payloadJson = Encoding.UTF8.GetString(payload);
            var receivedPayload = JsonConvert.DeserializeObject<RequestPayload>(payloadJson);

            Assert.NotNull(receivedPayload);

            Assert.Equal(request.Path, receivedPayload.Path);
            Assert.Equal(request.Verb, receivedPayload.Verb);

            Assert.Equal(request.Streams.Count, receivedPayload.Streams.Count);

            for (int i = 0; i < request.Streams.Count; i++)
            {
                Assert.Equal(request.Streams[i].ContentType, receivedPayload.Streams[i].ContentType);
                Assert.Equal(request.Streams[i].Id, receivedPayload.Streams[i].Id);
                Assert.Equal(request.Streams[i].Length, receivedPayload.Streams[i].Length);
            }
        }

        [Fact]
        public async Task TransportHandler_SendResponse_TransportReceivesHeaderAndPayload()
        {
            var response = new ResponseModel()
            {
                StatusCode = 200,
                Streams = new List<StreamDescription>()
                {
                    new StreamDescription() { ContentType = "json", Id = Guid.NewGuid().ToString(), Length = 18 },
                    new StreamDescription() { ContentType = "text", Id = Guid.NewGuid().ToString(), Length = 24 }
                }
            };

            var pipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            var transportHandler = new TransportHandler(pipePair.Transport, NullLogger.Instance);

            var transport = pipePair.Application.Input;

            await transportHandler.SendResponseAsync(Guid.NewGuid(), response, CancellationToken.None);

            var result = await transport.ReadAsync();
            var buffer = result.Buffer;

            var headerBuffer = buffer.Slice(0, Math.Min(TransportConstants.MaxHeaderLength, buffer.Length));

            var header = HeaderSerializer.Deserialize(headerBuffer.ToArray(), 0, TransportConstants.MaxHeaderLength);

            buffer = buffer.Slice(TransportConstants.MaxHeaderLength);

            if (buffer.Length < header.PayloadLength)
            {
                transport.AdvanceTo(buffer.Start, buffer.End);

                result = await transport.ReadAsync();

                Assert.False(result.IsCanceled);

                buffer = result.Buffer;
            }

            var payload = buffer.Slice(buffer.Start, header.PayloadLength).ToArray();

            var payloadJson = Encoding.UTF8.GetString(payload);
            var receivedPayload = JsonConvert.DeserializeObject<ResponsePayload>(payloadJson);

            Assert.NotNull(receivedPayload);

            Assert.Equal(response.StatusCode, receivedPayload.StatusCode);

            Assert.Equal(response.Streams.Count, receivedPayload.Streams.Count);

            for (int i = 0; i < response.Streams.Count; i++)
            {
                Assert.Equal(response.Streams[i].ContentType, receivedPayload.Streams[i].ContentType);
                Assert.Equal(response.Streams[i].Id, receivedPayload.Streams[i].Id);
                Assert.Equal(response.Streams[i].Length, receivedPayload.Streams[i].Length);
            }
        }

        [Fact]
        public async Task TransportHandler_SendStream_TransportReceivesHeaderAndPayload()
        {
            var text = "Hello human, I'm Bender";

            // TODO: make this a theory with increasing byte count. Implement chunking in the transport handler
            // to ensure once byte size increases we still send manageable packet size, and test the chunking here.
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));

            var pipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            var transportHandler = new TransportHandler(pipePair.Transport, NullLogger.Instance);

            var transport = pipePair.Application.Input;

            await transportHandler.SendStreamAsync(Guid.NewGuid(), stream, CancellationToken.None);

            var result = await transport.ReadAsync();
            var buffer = result.Buffer;

            var headerBuffer = buffer.Slice(0, Math.Min(TransportConstants.MaxHeaderLength, buffer.Length));

            var header = HeaderSerializer.Deserialize(headerBuffer.ToArray(), 0, TransportConstants.MaxHeaderLength);

            buffer = buffer.Slice(TransportConstants.MaxHeaderLength);

            if (buffer.Length < header.PayloadLength)
            {
                transport.AdvanceTo(buffer.Start, buffer.End);

                result = await transport.ReadAsync();

                Assert.False(result.IsCanceled);

                buffer = result.Buffer;
            }

            var payload = buffer.Slice(buffer.Start, header.PayloadLength).ToArray();

            var payloadString = Encoding.UTF8.GetString(payload);

            Assert.Equal(text, payloadString);
        }

        [Fact]
        public async Task TransportHandler_SendResponse_ThrowsOnNull()
        {
            var pipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            var transportHandler = new TransportHandler(pipePair.Transport, NullLogger.Instance);
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await transportHandler.SendResponseAsync(Guid.NewGuid(), null, CancellationToken.None));
        }

        [Fact]
        public async Task TransportHandler_SendStream_ThrowsOnNull()
        {
            var pipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            var transportHandler = new TransportHandler(pipePair.Transport, NullLogger.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await transportHandler.SendStreamAsync(Guid.NewGuid(), null, CancellationToken.None));
        }

        private static async Task RunTransportHandlerReceiveTestAsync(
            List<(Header Header, byte[] Payload)> transportData,
            bool cancelAfterFirst,
            bool cancelWithoutPayload,
            bool completeWithException)
        {
            var pipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            var applicationDuplexPipe = pipePair.Application;

            var transportHandler = new TransportHandler(pipePair.Transport, NullLogger.Instance);

            var transportObserver = new TestTransportObserver();
            transportHandler.Subscribe(transportObserver);

            var transportTask = transportHandler.ListenAsync(CancellationToken.None);

            var output = applicationDuplexPipe.Output;
            bool first = true;

            foreach (var entry in transportData)
            {
                var headerBuffer = new byte[48];
                HeaderSerializer.Serialize(entry.Header, headerBuffer, 0);

                Assert.Equal(entry.Header.PayloadLength, entry.Payload.Length);

                await output.WriteAsync(headerBuffer, CancellationToken.None);

                if (cancelWithoutPayload)
                {
                    output.CancelPendingFlush();
                    break;
                }

                if (entry.Header.PayloadLength > 0)
                {
                    await output.WriteAsync(entry.Payload, CancellationToken.None);
                }

                if (first && cancelAfterFirst)
                {
                    output.CancelPendingFlush();
                    break;
                }
            }

            if (completeWithException)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                await output.CompleteAsync(new Exception());
                await Assert.ThrowsAsync<Exception>(async () => await transportTask);
            }
            else
            {
                await output.CompleteAsync();
                
                if (Debugger.IsAttached)
                {
                    await transportTask;
                }
                else
                {
                    var result = await Task.WhenAny(transportTask, Task.Delay(TimeSpan.FromSeconds(5)));
                    Assert.Equal(result, transportTask);
                }
            }

            var receivedData = transportObserver.Received;

            if (cancelAfterFirst)
            {
                Assert.Single(receivedData);
            }
            else if (cancelWithoutPayload)
            {
                Assert.Empty(receivedData);
            }
            else if (!completeWithException)
            {
                Assert.Equal(transportData.Count, receivedData.Count);
            }

            if (!cancelAfterFirst && !cancelWithoutPayload && !completeWithException)
            {
                for (int i = 0; i < transportData.Count; i++)
                {
                    Assert.Equal(transportData[i].Header.End, receivedData[i].Header.End);
                    Assert.Equal(transportData[i].Header.Id, receivedData[i].Header.Id);
                    Assert.Equal(transportData[i].Header.PayloadLength, receivedData[i].Header.PayloadLength);
                    Assert.Equal(transportData[i].Header.Type, receivedData[i].Header.Type);

                    Assert.True(transportData[i].Payload.SequenceEqual(receivedData[i].Payload));
                }
            }
        }

        private static List<(Header Header, byte[] Payload)> GenerateHeaderPayloadData(int totalLength, int packageCount)
        {
            var result = new List<(Header Header, byte[] Payload)>();

            if (totalLength == 0)
            { 
                var header = new Header()
                {
                    Id = Guid.NewGuid(),
                    End = true,
                    PayloadLength = 0,
                    Type = PayloadTypes.Stream
                };

                result.Add((header, null));
                return result; 
            }

            byte[] buffer = new byte[totalLength];

            var random = new Random();
            random.NextBytes(buffer);

            var chunkSize = totalLength / packageCount;

            var current = 0;

            while (current < totalLength)
            {
                var currentSize = Math.Min(chunkSize, totalLength - current);

                var header = new Header()
                {
                    Id = Guid.NewGuid(),
                    End = true,
                    PayloadLength = currentSize,
                    Type = PayloadTypes.Stream
                };

                var payload = new byte[currentSize];

                result.Add((header, payload));

                current += currentSize;
            }

            return result;
        }
    }
}
