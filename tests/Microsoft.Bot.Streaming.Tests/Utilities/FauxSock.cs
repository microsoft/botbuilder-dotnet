// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Tests.Utilities
{
    public class FauxSock : WebSocket
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            ReceivedArray = buffer;

            return new WebSocketReceiveResult(buffer.Count, WebSocketMessageType.Close, true);
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            SentArray = buffer;

            return Task.CompletedTask;
        }
    }
}
