// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.WebSockets;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Microsoft.Bot.Streaming.Transport.WebSockets;

namespace Microsoft.Bot.Builder.Streaming
{
    internal sealed class StreamingTransportServerFactory : IStreamingTransportServerFactory
    {
        private readonly string _pipeName;
        private readonly WebSocket _socket;

        public StreamingTransportServerFactory(WebSocket socket)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        }

        public StreamingTransportServerFactory(string pipeName)
        {
            _pipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
        }

        public IStreamingTransportServer Create(RequestHandler requestHandler)
        {
            if (_socket != null)
            {
                return new WebSocketServer(_socket, requestHandler);
            }

            if (!string.IsNullOrWhiteSpace(_pipeName))
            {
                return new NamedPipeServer(_pipeName, requestHandler);
            }

            throw new ApplicationException("Neither web socket, nor named pipe found to instantiate a streaming transport server!");
        }
    }
}
