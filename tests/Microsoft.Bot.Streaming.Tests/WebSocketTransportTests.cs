// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Streaming;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Microsoft.Bot.Streaming.Transport.WebSockets;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class WebSocketTransportTests
    {
        [Fact]
        public async Task WebSocketServer_Connects()
        {
            var sock = new FauxSock();
            var writer = new WebSocketServer(sock, new StreamingRequestHandler(null, new DirectLineAdapter(), sock));

            writer.StartAsync();
            Assert.True(writer.IsConnected);
        }

        [Fact]
        public async Task WebSocketServer_BackAndForth()
        {
            var sock = new FauxSock();
            var writer = new WebSocketServer(sock, new StreamingRequestHandler(null, new DirectLineAdapter(), sock));

            writer.StartAsync();
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

        private class FauxSock : WebSocket
        {
            public ArraySegment<byte> SentArray { get; set; }

            public override WebSocketCloseStatus? CloseStatus => throw new NotImplementedException();

            public override string CloseStatusDescription => throw new NotImplementedException();

            public override WebSocketState State => throw new NotImplementedException();

            public override string SubProtocol => throw new NotImplementedException();

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
    }
}
