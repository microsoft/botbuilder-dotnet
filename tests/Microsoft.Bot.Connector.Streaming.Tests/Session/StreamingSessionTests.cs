// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Connector.Streaming.Payloads;
using Microsoft.Bot.Connector.Streaming.Session;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using static Microsoft.Bot.Connector.Streaming.Session.StreamingSession;
using RequestModel = Microsoft.Bot.Connector.Streaming.Payloads.RequestPayload;
using ResponseModel = Microsoft.Bot.Connector.Streaming.Payloads.ResponsePayload;

namespace Microsoft.Bot.Connector.Streaming.Tests
{
    public class StreamingSessionTests
    {
        public static IEnumerable<object[]> ReceiveRequestParameterValidationData =>
            new List<object[]>
            {
                new object[] { PayloadTypes.Request, null, typeof(ArgumentNullException) },
                new object[] { '\0', new ReceiveRequest(), typeof(ArgumentNullException) },
                new object[] { PayloadTypes.Response, new ReceiveRequest(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.Stream, new ReceiveRequest(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.CancelStream, new ReceiveRequest(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.CancelAll, new ReceiveRequest(), typeof(InvalidOperationException) }
            };

        public static IEnumerable<object[]> ReceiveResponseParameterValidationData =>
            new List<object[]>
            {
                new object[] { PayloadTypes.Response, null, typeof(ArgumentNullException) },
                new object[] { '\0', new ReceiveResponse(), typeof(ArgumentNullException) },
                new object[] { PayloadTypes.Request, new ReceiveResponse(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.Stream, new ReceiveResponse(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.CancelStream, new ReceiveResponse(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.CancelAll, new ReceiveResponse(), typeof(InvalidOperationException) },
            };

        public static IEnumerable<object[]> ReceiveStreamParameterValidationData =>
            new List<object[]>
            {
                new object[] { PayloadTypes.Response, null, typeof(ArgumentNullException) },
                new object[] { '\0', Array.Empty<byte>(), typeof(ArgumentNullException) },
                new object[] { PayloadTypes.Request, Array.Empty<byte>(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.Response, Array.Empty<byte>(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.CancelStream, Array.Empty<byte>(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.CancelAll, Array.Empty<byte>(), typeof(InvalidOperationException) },
            };

        public static IEnumerable<object[]> SendResponseParameterValidationData =>
            new List<object[]>
            {
                new object[] { PayloadTypes.Response, null, typeof(ArgumentNullException) },
                new object[] { '\0', new StreamingResponse(), typeof(ArgumentNullException) },
                new object[] { PayloadTypes.Request, new StreamingResponse(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.Stream, new StreamingResponse(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.CancelAll, new StreamingResponse(), typeof(InvalidOperationException) },
                new object[] { PayloadTypes.CancelStream, new StreamingResponse(), typeof(InvalidOperationException) },
            };

        [Fact]
        public void StreamingSession_Constructor_NullRequestHandler_Throws()
        {
            var transportHandler = new Mock<TransportHandler>(new Mock<IDuplexPipe>().Object, NullLogger.Instance);
            Assert.Throws<ArgumentNullException>(
                () => new StreamingSession(null, transportHandler.Object, NullLogger.Instance));
        }

        [Fact]
        public void StreamingSession_Constructor_NullTransportHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => new StreamingSession(new Mock<RequestHandler>().Object, null, NullLogger.Instance));
        }

        [Fact]
        public async Task StreamingSession_SendRequest_ParameterValidation()
        {
            // Arrange
            var transportHandler = new Mock<TransportHandler>(new Mock<IDuplexPipe>().Object, NullLogger.Instance);
            var session = new StreamingSession(new Mock<RequestHandler>().Object, transportHandler.Object, NullLogger.Instance);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => session.SendRequestAsync(null, CancellationToken.None));
        }

        [Theory]
        [MemberData(nameof(SendResponseParameterValidationData))]
        public async Task StreamingSession_SendResponse_ParameterValidation(char payloadType, StreamingResponse response, Type exceptionType)
        {
            // Arrange
            var transportHandler = new Mock<TransportHandler>(new Mock<IDuplexPipe>().Object, NullLogger.Instance);
            var session = new StreamingSession(new Mock<RequestHandler>().Object, transportHandler.Object, NullLogger.Instance);

            // Act + Assert
            var header = payloadType == '\0' ? null : new Header { Type = payloadType };
            await Assert.ThrowsAsync(exceptionType, () => session.SendResponseAsync(header, response, CancellationToken.None));
        }

        [Theory]
        [MemberData(nameof(ReceiveRequestParameterValidationData))]
        public void StreamingSession_ReceiveRequest_ParameterValidation(char payloadType, ReceiveRequest request, Type exceptionType)
        {
            // Arrange
            var transportHandler = new Mock<TransportHandler>(new Mock<IDuplexPipe>().Object, NullLogger.Instance);
            var session = new StreamingSession(new Mock<RequestHandler>().Object, transportHandler.Object, NullLogger.Instance);

            // Act + Assert
            var header = payloadType == '\0' ? null : new Header { Type = payloadType };
            Assert.Throws(exceptionType, () => session.ReceiveRequest(header, request));
        }

        [Theory]
        [MemberData(nameof(ReceiveResponseParameterValidationData))]
        public void StreamingSession_ReceiveResponse_ParameterValidation(char payloadType, ReceiveResponse response, Type exceptionType)
        {
            // Arrange
            var transportHandler = new Mock<TransportHandler>(new Mock<IDuplexPipe>().Object, NullLogger.Instance);
            var session = new StreamingSession(new Mock<RequestHandler>().Object, transportHandler.Object, NullLogger.Instance);

            // Act + Assert
            var header = payloadType == '\0' ? null : new Header { Type = payloadType };
            Assert.Throws(exceptionType, () => session.ReceiveResponse(header, response));
        }

        [Theory]
        [MemberData(nameof(ReceiveStreamParameterValidationData))]
        public void StreamingSession_ReceiveStream_ParameterValidation(char payloadType, byte[] payload, Type exceptionType)
        {
            // Arrange
            var transportHandler = new Mock<TransportHandler>(new Mock<IDuplexPipe>().Object, NullLogger.Instance);
            var session = new StreamingSession(new Mock<RequestHandler>().Object, transportHandler.Object, NullLogger.Instance);

            // Act + Assert
            var header = payloadType == '\0' ? null : new Header { Type = payloadType };
            Assert.Throws(exceptionType, () => session.ReceiveStream(header, new ArraySegment<byte>(payload)));
        }

        [Theory]
        [InlineData(10, 1, 1)]
        [InlineData(100, 1, 1)]
        [InlineData(1000, 1, 1)]
        [InlineData(10000, 1, 1)]
        [InlineData(1000, 2, 1)]
        [InlineData(1000, 1, 2)]
        [InlineData(1000, 1, 10)]
        [InlineData(1000, 10, 10)]
        [InlineData(1000, 100, 10)]
        public async Task StreamingSession_RequestWithStreams_SentToHandler(int streamLength, int streamCount, int chunkCount)
        {
            // Arrange
            var requestId = Guid.NewGuid();

            var request = new ReceiveRequest()
            {
                Verb = "GET",
                Path = "api/version",
            };

            request.Streams.AddRange(StreamingDataGenerator.CreateStreams(requestId, streamLength, streamCount, chunkCount));

            var requestHandler = new Mock<RequestHandler>();

            var requestCompletionSource = new TaskCompletionSource<bool>();

            requestHandler
                .Setup(r => r.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), null, null, CancellationToken.None))
                .ReturnsAsync(() => new StreamingResponse() { StatusCode = 200 })
                .Callback(() => requestCompletionSource.SetResult(true));

            var transportHandler = new Mock<TransportHandler>(new Mock<IDuplexPipe>().Object, NullLogger.Instance);

            var responseCompletionSource = new TaskCompletionSource<bool>();

            transportHandler
                .Setup(t => t.SendResponseAsync(It.IsAny<Guid>(), It.Is<ResponseModel>(r => r.StatusCode == 200), CancellationToken.None))
                .Callback(() => responseCompletionSource.SetResult(true));

            // Act
            var session = new StreamingSession(requestHandler.Object, transportHandler.Object, NullLogger.Instance);

            session.ReceiveRequest(new Header() { Id = requestId, Type = PayloadTypes.Request }, request);

            foreach (AugmentedStreamDefinition definition in request.Streams)
            {
                var chunkList = definition.Chunks;

                for (int i = 0; i < chunkList.Count; i++)
                {
                    bool isLast = i == chunkList.Count - 1;
                    
                    session.ReceiveStream(
                        new Header() { End = isLast, Id = definition.Id, PayloadLength = chunkList[i].Length, Type = PayloadTypes.Stream },
                        chunkList[i]);
                }
            }

            var roundtripTask = Task.WhenAll(requestCompletionSource.Task, responseCompletionSource.Task);
            var result = await Task.WhenAny(roundtripTask, Task.Delay(TimeSpan.FromSeconds(5)));

            // Assert
            Assert.Equal(result, roundtripTask);
        }

        [Theory]
        [InlineData(10, 1, 1)]
        [InlineData(100, 1, 1)]
        [InlineData(1000, 1, 1)]
        [InlineData(10000, 1, 1)]
        [InlineData(1000, 2, 1)]
        [InlineData(1000, 1, 2)]
        [InlineData(1000, 1, 10)]
        [InlineData(1000, 10, 10)]
        [InlineData(1000, 100, 10)]
        public async Task StreamingSession_SendRequest_ReceiveResponse(int streamLength, int streamCount, int chunkCount)
        {
            // Arrange
            var request = new StreamingRequest()
            {
                Verb = "GET",
                Path = "api/version",
            };

            request.AddStream(new StringContent("Hello human, I'm Bender!"));

            var requestHandler = new Mock<RequestHandler>();

            var requestCompletionSource = new TaskCompletionSource<bool>();

            requestHandler
                .Setup(r => r.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), null, null, CancellationToken.None))
                .ReturnsAsync(() => new StreamingResponse() { StatusCode = 200 })
                .Callback(() => requestCompletionSource.SetResult(true));

            var transportHandler = new Mock<TransportHandler>(new Mock<IDuplexPipe>().Object, NullLogger.Instance);

            var responseCompletionSource = new TaskCompletionSource<bool>();

            var transportHandlerSetup = transportHandler.Setup(t => t.SendRequestAsync(It.IsAny<Guid>(), It.IsAny<RequestModel>(), CancellationToken.None));

            var session = new StreamingSession(requestHandler.Object, transportHandler.Object, NullLogger.Instance);

            Header responseHeader = null;
            ReceiveResponse response = null;

            transportHandlerSetup.Callback(
                (Guid requestId, RequestModel requestPayload, CancellationToken cancellationToken) =>
                {
                    responseHeader = new Header() { Id = requestId, Type = PayloadTypes.Response };
                    response = new ReceiveResponse() { StatusCode = 200, };
                    response.Streams.AddRange(StreamingDataGenerator.CreateStreams(requestId, streamLength, streamCount, chunkCount, PayloadTypes.Response));

                    session.ReceiveResponse(responseHeader, response);

                    foreach (AugmentedStreamDefinition definition in response.Streams)
                    {
                        var chunkList = definition.Chunks;

                        for (int i = 0; i < chunkCount; i++)
                        {
                            bool isLast = i == chunkCount - 1;

                            session.ReceiveStream(
                                new Header() { End = isLast, Id = definition.Id, PayloadLength = chunkList[i].Length, Type = PayloadTypes.Stream },
                                chunkList[i]);
                        }
                    }
                });

            // Act

            var responseTask = session.SendRequestAsync(request, CancellationToken.None);
            var responseWithTimeout = await Task.WhenAny(responseTask, Task.Delay(TimeSpan.FromSeconds(5)));

            // Assert
            Assert.Equal(responseTask, responseWithTimeout);

            var receivedResponse = await responseTask;

            Assert.Equal(response.StatusCode, receivedResponse.StatusCode);
            Assert.Equal(response.Streams.Count, receivedResponse.Streams.Count);

            Assert.True(response.Streams.SequenceEqual(receivedResponse.Streams));
        }

        internal static class StreamingDataGenerator
        {
            public static List<IContentStream> CreateStreams(Guid requestId, int streamLength, int streamCount = 1, int chunkCount = 1, char type = PayloadTypes.Request)
            {
                var result = new List<IContentStream>();

                for (int i = 0; i < streamCount; i++)
                {
                    // To keep code simple, asking that stream length can be equally divided in chunks. Feel
                    // free to adapt code to support it if needed.
                    Assert.Equal(0, streamLength % chunkCount);

                    var definition = new AugmentedStreamDefinition()
                    {
                        Complete = false,
                        Id = Guid.NewGuid(),
                        PayloadId = requestId,
                        Length = streamLength,
                        PayloadType = type,
                        Stream = new MemoryStream()
                    };

                    int chunkSize = streamLength / chunkCount;
                    int current = 0;

                    while (current < streamLength)
                    {
                        var data = new byte[chunkSize];
                        new Random().NextBytes(data);

                        definition.Chunks.Add(data);

                        current += chunkSize;
                    }

                    result.Add(definition);
                }

                return result;
            }
        }

        private class AugmentedStreamDefinition : StreamDefinition
        {
            public List<byte[]> Chunks { get; set; } = new List<byte[]>();
        }

        //[Fact]
        //public async Task StreamingSession_RequestWithNoStreams_SentToHandler()
        //{ 
        //}

        //[Fact]
        //public async Task StreamingSession_ResponseWithNoStreams_SentToHandler()
        //{ 
        
        //}

        //[Fact]
        //public async Task StreamingSession_ResponseWithStreams_SentToHandler()
        //{ 
        
        //}

        //[Fact]
        //public async Task StreamingSession_SendRequest_ResponseReceivedAsynchronously()
        //{ 
        
        //}

        //[Fact]
        //public async Task StreamingSession_SendResponse_Succeeds()
        //{ 
            
        //}
    }
}
