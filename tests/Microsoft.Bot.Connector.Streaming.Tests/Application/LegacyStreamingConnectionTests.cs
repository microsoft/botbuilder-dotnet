// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Microsoft.Bot.Streaming.Transport.WebSockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Bot.Connector.Streaming.Tests.Application
{
    public class LegacyStreamingConnectionTests
    {
        [Fact]
        public void ConstructorTests()
        {
            var webSocketConnection = new LegacyStreamingConnection(new TestWebSocket(), null);
            var namedPipeConnection = new LegacyStreamingConnection("test", null);
        }

        [Fact]
        public void CannotCreateWithoutValidWebSocket()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                WebSocket socket = null;
                _ = new LegacyStreamingConnection(socket, NullLogger.Instance);
            });
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void CannotCreateWithoutValidPipeName(string pipeName)
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new LegacyStreamingConnection(pipeName, NullLogger.Instance);
            });
        }

        [Fact]
        public void CanCreateWebSocketServer()
        {
            var socket = new TestWebSocket();
            var requestHandler = new TestRequestHandler();

            var sut = new LegacyStreamingConnection(socket, NullLogger.Instance);

            var server = sut.CreateStreamingTransportServer(requestHandler);
            Assert.True(server is WebSocketServer);
        }

        [Fact]
        public void CanCreateNamedPipeServer()
        {
            var requestHandler = new TestRequestHandler();

            var sut = new LegacyStreamingConnection("test", NullLogger.Instance);

            var server = sut.CreateStreamingTransportServer(requestHandler);
            Assert.True(server is NamedPipeServer);
        }

        [Fact]
        public async Task CanSendStreamingRequestAsync()
        {
            var socket = new TestWebSocket();
            var requestHandler = new TestRequestHandler();

            using (var sut = new TestLegacyStreamingConnection(socket, NullLogger.Instance))
            {
                await sut.ListenAsync(requestHandler);

                var request = new StreamingRequest
                {
                    Verb = "POST",
                    Path = "/api/messages",
                    Streams = new List<ResponseMessageStream>
                    {
                        new ResponseMessageStream { Content = new StringContent("foo") }
                    }
                };

                var response = await sut.SendStreamingRequestAsync(request);

                Assert.Equal(request.Streams.Count, response.Streams.Count);
                Assert.Equal(request.Streams[0].Id, response.Streams[0].Id);
            }
        }

        private class TestLegacyStreamingConnection : LegacyStreamingConnection
        {
            public TestLegacyStreamingConnection(WebSocket socket, ILogger logger, DisconnectedEventHandler onServerDisconnect = null)
                : base(socket, logger, onServerDisconnect)
            {
            }

            public TestLegacyStreamingConnection(string pipeName, ILogger logger, DisconnectedEventHandler onServerDisconnect = null)
                : base(pipeName, logger, onServerDisconnect)
            {
            }

            internal override IStreamingTransportServer CreateStreamingTransportServer(RequestHandler requestHandler)
            {
                return new TestStreamingTransportServer();
            }
        }

        private class TestStreamingTransportServer : IStreamingTransportServer, IDisposable
        {
            public event DisconnectedEventHandler Disconnected;

            public Task StartAsync()
            {
                return Task.CompletedTask;
            }

            public Task<ReceiveResponse> SendAsync(StreamingRequest request, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(new ReceiveResponse
                {
                    StatusCode = 200,
                    Streams = new List<IContentStream>(request.Streams.Select(s => new TestContentStream(s.Id)))
                });
            }

            public void Dispose()
            {
                if (Disconnected != null)
                {
                    Disconnected(this, DisconnectedEventArgs.Empty);
                }
            }
        }

        private class TestWebSocket : WebSocket
        {
            public override WebSocketCloseStatus? CloseStatus { get; }

            public override string CloseStatusDescription { get; }

            public override WebSocketState State { get; }

            public override string SubProtocol { get; }

            public override void Abort()
            {
                throw new NotImplementedException();
            }

            public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }

            public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class TestRequestHandler : RequestHandler
        {
            public override Task<StreamingResponse> ProcessRequestAsync(ReceiveRequest request, ILogger<RequestHandler> logger, object context = null, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(new StreamingResponse { StatusCode = 200 });
            }
        }

        private class TestContentStream : IContentStream
        {
            public TestContentStream(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; }

            public string ContentType { get; set; }
            
            public int? Length { get; set; }
            
            public Stream Stream { get; }
        }
    }
}
