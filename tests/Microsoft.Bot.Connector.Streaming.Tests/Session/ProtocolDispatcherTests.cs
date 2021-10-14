// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Bot.Connector.Streaming.Session;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Xunit;
using static Microsoft.Bot.Connector.Streaming.Session.StreamingSession;

namespace Microsoft.Bot.Connector.Streaming.Tests
{
    public class ProtocolDispatcherTests
    {
        [Fact]
        public void ProtocolDispatcher_NullSession_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ProtocolDispatcher(null));
        }

        [Fact]
        public void ProtocolDispatcher_DispatchRequest()
        {
            // Arrange
            var request = new RequestPayload()
            {
                Verb = "GET",
                Path = "api/version",
                Streams = new List<StreamDescription>()
                {
                    new StreamDescription() { ContentType = "json", Id = Guid.NewGuid().ToString(), Length = 18 },
                    new StreamDescription() { ContentType = "text", Id = Guid.NewGuid().ToString(), Length = 24 }
                }
            };

            var requestJson = JsonConvert.SerializeObject(request);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);

            var header = new Header()
            {
                End = true,
                Id = Guid.NewGuid(),
                PayloadLength = requestBytes.Length,
                Type = PayloadTypes.Request
            };

            var callCount = 0;

            var transportHandler = new Mock<TransportHandler>(new Mock<IDuplexPipe>().Object, NullLogger.Instance);
            var requestHandler = new Mock<RequestHandler>();

            var session = new Mock<StreamingSession>(requestHandler.Object, transportHandler.Object, NullLogger.Instance, CancellationToken.None);

            session.Setup(
                s => s.ReceiveRequest(It.IsAny<Header>(), It.IsAny<ReceiveRequest>()))
                    .Callback((Header h, ReceiveRequest r) =>
                    {
                        callCount++;

                        // Assert
                        Assert.Equal(h.Id, header.Id);
                        Assert.Equal(request.Verb, r.Verb);
                        Assert.Equal(request.Path, r.Path);
                        Assert.Equal(request.Streams.Count, r.Streams.Count);

                        var firstStream = r.Streams.First() as StreamDefinition;
                        Assert.Equal(request.Streams.First().Id, firstStream.Id.ToString());
                        Assert.Equal(request.Streams.First().Length, firstStream.Length);
                        Assert.IsType<MemoryStream>(firstStream.Stream);
                        Assert.Equal(h.Id, firstStream.PayloadId);
                    });

            var dispatcher = new ProtocolDispatcher(session.Object);

            // Act
            dispatcher.OnNext((header, new ReadOnlySequence<byte>(requestBytes)));

            // Assert
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void ProtocolDispatcher_DispatchResponse()
        {
            // Arrange
            var request = new ResponsePayload()
            {
                StatusCode = 200,
                Streams = new List<StreamDescription>()
                {
                    new StreamDescription() { ContentType = "json", Id = Guid.NewGuid().ToString(), Length = 18 },
                    new StreamDescription() { ContentType = "text", Id = Guid.NewGuid().ToString(), Length = 24 }
                }
            };

            var requestJson = JsonConvert.SerializeObject(request);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);

            var header = new Header()
            {
                End = true,
                Id = Guid.NewGuid(),
                PayloadLength = requestBytes.Length,
                Type = PayloadTypes.Response
            };

            var callCount = 0;

            var transportHandler = new Mock<TransportHandler>(new Mock<IDuplexPipe>().Object, NullLogger.Instance);
            var requestHandler = new Mock<RequestHandler>();

            var session = new Mock<StreamingSession>(requestHandler.Object, transportHandler.Object, NullLogger.Instance, CancellationToken.None);

            session.Setup(
                s => s.ReceiveResponse(It.IsAny<Header>(), It.IsAny<ReceiveResponse>()))
                    .Callback((Header h, ReceiveResponse r) =>
                    {
                        callCount++;

                        // Assert
                        Assert.Equal(h.Id, header.Id);
                        Assert.Equal(request.StatusCode, r.StatusCode);
                        Assert.Equal(request.Streams.Count, r.Streams.Count);

                        var firstStream = r.Streams.First() as StreamDefinition;
                        Assert.Equal(request.Streams.First().Id, firstStream.Id.ToString());
                        Assert.Equal(request.Streams.First().Length, firstStream.Length);
                        Assert.IsType<MemoryStream>(firstStream.Stream);
                        Assert.Equal(0, firstStream.Stream.Length);
                    });

            var dispatcher = new ProtocolDispatcher(session.Object);

            // Act
            dispatcher.OnNext((header, new ReadOnlySequence<byte>(requestBytes)));

            // Assert
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void ProtocolDispatcher_DispatchStream()
        {
            // Arrange
            var buffer = new byte[256];
            new Random().NextBytes(buffer);

            var header = new Header()
            {
                End = true,
                Id = Guid.NewGuid(),
                PayloadLength = buffer.Length,
                Type = PayloadTypes.Stream
            };

            var callCount = 0;

            var transportHandler = new Mock<TransportHandler>(new Mock<IDuplexPipe>().Object, NullLogger.Instance);
            var requestHandler = new Mock<RequestHandler>();

            var session = new Mock<StreamingSession>(requestHandler.Object, transportHandler.Object, NullLogger.Instance, CancellationToken.None);

            session
                .Setup(s => s.ReceiveStream(It.IsAny<Header>(), It.IsAny<ArraySegment<byte>>()))
                .Callback((Header h, ArraySegment<byte> s) =>
                {
                    callCount++;

                    // Assert
                    Assert.Equal(h.Id, header.Id);
                    Assert.True(s.Array.SequenceEqual(buffer));
                });

            var dispatcher = new ProtocolDispatcher(session.Object);

            // Act
            dispatcher.OnNext((header, new ReadOnlySequence<byte>(buffer)));

            // Assert
            Assert.Equal(1, callCount);
        }
    }
}
