// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.Transport.WebSockets;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class WebSocketTransportTests
    {
        [Fact]
        public async Task WebSocketServer_Connects()
        {
            // Arrange
            var requestHandlerMock = new Mock<RequestHandler>();
            requestHandlerMock.Setup(
                rh => rh.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), It.IsAny<ILogger<RequestHandler>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StreamingResponse { StatusCode = 200 });

            // TaskCompletionSource will complete when the first receive is executed and this must happen AFTER the IsConnected state is set
            var sock = new FauxSocketTaskCompletionSource();

            // Set the faux-socket state because otherwise the background task might check it and disconnected before this test completes
            sock.RealState = WebSocketState.Open;

            var writer = new WebSocketServer(sock, requestHandlerMock.Object);

            // Act

            var task = writer.StartAsync();

            // wait for a socket receive to actually happen to eliminate a race
            await sock.TaskCompletionSource.Task;

            // now check the connected state
            bool isConnectedAfterStart = writer.IsConnected;

            // we should be able to safely wait for the Start to complete now
            await task;

            // disconnect will stop the receive loop
            writer.Disconnect();

            // now check the connected state
            bool isConnectedAfterDisconnect = writer.IsConnected;

            // Assert
            Assert.True(isConnectedAfterStart);
            Assert.False(isConnectedAfterDisconnect);
        }

        [Fact]
        public async Task WebSocketClient_ThrowsOnEmptyUrl()
        {
            Exception result = null;

            try
            {
                var reader = new WebSocketClient(string.Empty);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            Assert.IsType<ArgumentNullException>(result);
        }

        [Fact]
        public async Task WebSocketClient_AcceptsAnyUrl()
        {
            Exception result = null;
            WebSocketClient reader = null;
            try
            {
                var webSocketClient = new WebSocketClient("fakeurl");
                reader = webSocketClient;
            }
            catch (Exception ex)
            {
                result = ex;
            }

            reader.Dispose();
            Assert.Null(result);
        }

        [Fact]
        public async Task WebSocketClient_ConnectThrowsIfUrlIsBad()
        {
            Exception result = null;
            WebSocketClient reader = null;
            IDictionary<string, string> fakeHeaders = new Dictionary<string, string>();
            fakeHeaders.Add("authorization", "totally");
            fakeHeaders.Add("channelId", "mtv");
            try
            {
                var webSocketClient = new WebSocketClient("fakeurl");
                reader = webSocketClient;
                await reader.ConnectAsync(fakeHeaders);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            reader.Dispose();
            Assert.IsType<UriFormatException>(result);
        }

        [Fact]
        public async Task WebSocketClient_ConnectAsyncExThrowsIfUrlIsBad()
        {
            WebSocketClient reader = null;
            using var webSocketClient = new WebSocketClient("fakeurl");
            reader = webSocketClient;
            await Assert.ThrowsAsync<UriFormatException>(async () => await reader.ConnectAsyncEx(null, CancellationToken.None));
        }

        [Fact]
        public async Task WebSocketTransport_Connects()
        {
            var sock = new FauxSock();
            sock.RealState = WebSocketState.Open;
            var transport = new WebSocketTransport(sock);

            Assert.True(transport.IsConnected);

            transport.Close();
            transport.Dispose();
        }

        [Fact]
        public async Task WebSocketTransport_SetsState()
        {
            var sock = new FauxSock();
            sock.RealState = WebSocketState.Open;
            var transport = new WebSocketTransport(sock);

            transport.Close();
            transport.Dispose();

            Assert.Equal(WebSocketState.Closed, sock.RealState);
        }

        [Fact]
        public async Task WebSocketTransport_CanSend()
        {
            // Arrange
            var sock = new FauxSock();
            sock.RealState = WebSocketState.Open;
            var transport = new WebSocketTransport(sock);
            var messageText = "This is a message.";
            byte[] message = Encoding.ASCII.GetBytes(messageText);

            // Act
            await transport.SendAsync(message, 0, message.Length);

            // Assert
            Assert.Equal(messageText, Encoding.UTF8.GetString(sock.SentArray));
        }

        [Fact]
        public async Task WebSocketTransport_CanReceive()
        {
            // Arrange
            var sock = new FauxSock();
            sock.RealState = WebSocketState.Open;
            var transport = new WebSocketTransport(sock);
            byte[] message = Encoding.ASCII.GetBytes("This is a message.");

            // Act
            var received = await transport.ReceiveAsync(message, 0, message.Length);

            // Assert
            Assert.Equal(message.Length, received);
        }

        private class FauxSock : WebSocket
        {
            public ArraySegment<byte> SentArray { get; set; }

            public ArraySegment<byte> ReceivedArray { get; set; }

            public WebSocketState RealState { get; set; }

            public override WebSocketCloseStatus? CloseStatus => throw new NotImplementedException();

            public override string CloseStatusDescription => throw new NotImplementedException();

            public override WebSocketState State { get => RealState; }

            public override string SubProtocol => throw new NotImplementedException();

            public override void Abort()
            {
                throw new NotImplementedException();
            }

            public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
            {
                RealState = WebSocketState.Closed;

                return Task.CompletedTask;
            }

            public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
                RealState = WebSocketState.Closed;

                return;
            }

            public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
            {
                ReceivedArray = buffer;

                return new WebSocketReceiveResult(buffer.Count, WebSocketMessageType.Close, true);
            }

            public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
            {
                SentArray = buffer;

                return Task.CompletedTask;
            }
        }

        // This extends the basic faux socket with a TaskCompletionSource that can be used to block methods to add some synchronization to tests.
        private class FauxSocketTaskCompletionSource : FauxSock
        {
            public TaskCompletionSource<string> TaskCompletionSource { get; } = new TaskCompletionSource<string>();

            public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
            {
                // indicate to the test that receive has been called - to we should now be connected
                TaskCompletionSource.SetResult("ReceiveAsync");

                // delay so we don't thrash, the header we return with End=false will cause an infinite receive loop
                await Task.Delay(1000);

                // send a valid header so we don't immediate get a deserialization exception that triggers disconnect 
                var header = new Header() { Type = PayloadTypes.Stream, Id = Guid.NewGuid(), PayloadLength = 0, End = false };
                var count = HeaderSerializer.Serialize(header, buffer.Array, buffer.Offset);
                return new WebSocketReceiveResult(count, WebSocketMessageType.Binary, true);
            }
        }
    }
}
