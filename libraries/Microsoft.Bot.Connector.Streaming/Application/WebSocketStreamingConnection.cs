// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO.Pipelines;
using System.Net.WebSockets;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    /// <summary>
    /// Default implementation of <see cref="StreamingConnection"/> for WebSocket transport.
    /// </summary>
    public class WebSocketStreamingConnection : StreamingConnection
    {
        private readonly WebSocket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketStreamingConnection"/> class.
        /// </summary>
        /// <param name="socket"><see cref="WebSocket"/> instance on which streams are transported between client and server.</param>
        /// <param name="logger"><see cref="ILogger"/> for the connection.</param>
        public WebSocketStreamingConnection(WebSocket socket, ILogger logger)
            : base(logger)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        }

        internal override StreamingTransport CreateStreamingTransport(IDuplexPipe application)
        {
            return new WebSocketTransport(_socket, application, Logger);
        }
    }
}
