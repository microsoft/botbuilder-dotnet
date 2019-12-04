// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest.Serialization;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
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

        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            ReceivedArray = buffer;

            return Task.FromResult(new WebSocketReceiveResult(buffer.Count, WebSocketMessageType.Close, true));
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            SentArray = buffer;

            return Task.CompletedTask;
        }
    }
}
