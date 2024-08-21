// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.Tests.Utilities;
using Microsoft.Bot.Streaming.Transport.WebSockets;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class WebSocketClientServerTests
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
        public void WebSocketServer_ctor_With_No_Socket()
        {
            var requestHandlerMock = new Mock<RequestHandler>();

            Assert.Throws<ArgumentNullException>(() => new WebSocketServer(null, requestHandlerMock.Object));
        }

        [Fact]
        public void WebSocketServer_ctor_With_No_RequestHandler()
        {
            var requestHandlerMock = new Mock<RequestHandler>();
            var socketMock = new Mock<WebSocket>();

            Assert.Throws<ArgumentNullException>(() => new WebSocketServer(socketMock.Object, null));
        }

        [Fact]
        public async Task WebSocketServer_SendAsync_With_No_Message()
        {
            var socketMock = new Mock<WebSocket>();
            var requestHandlerMock = new Mock<RequestHandler>();
            var server = new WebSocketServer(socketMock.Object, requestHandlerMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => server.SendAsync(null));
        }

        [Fact]
        public async Task WebSocketServer_SendAsync_With_No_Connected_Client()
        {
            var socketMock = new Mock<WebSocket>();
            var requestHandlerMock = new Mock<RequestHandler>();
            var server = new WebSocketServer(socketMock.Object, requestHandlerMock.Object);
            var message = new StreamingRequest();

            await Assert.ThrowsAsync<InvalidOperationException>(() => server.SendAsync(message));
        }

        [Fact]
        public void WebSocketClient_ThrowsOnEmptyUrl()
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
        public async Task WebSocketClient_SendAsync_With_No_Message()
        {
            var client = new WebSocketClient("url");

            await Assert.ThrowsAsync<ArgumentNullException>(() => client.SendAsync(null));
        }

        [Fact]
        public async Task WebSocketClient_SendAsync_With_No_Connected_Client()
        {
            var client = new WebSocketClient("url");
            var message = new StreamingRequest();

            await Assert.ThrowsAsync<InvalidOperationException>(() => client.SendAsync(message));
        }

        [Fact]
        public void WebSocketClient_AcceptsAnyUrl()
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
