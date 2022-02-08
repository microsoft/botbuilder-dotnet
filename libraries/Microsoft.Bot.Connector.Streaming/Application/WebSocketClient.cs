// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO.Pipelines;
using System.Net.WebSockets;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Bot.Streaming;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    /// <summary>
    /// Web socket client.
    /// </summary>
    public class WebSocketClient : StreamingTransportClient
    {
        private readonly WebSocket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketClient"/> class.
        /// </summary>
        /// <param name="socket">The client web socket to initiate streaming connection to a server.</param>
        /// <param name="url">The server URL to connect to.</param>
        /// <param name="requestHandler">Handler that will receive incoming requests to this client instance.</param>
        /// <param name="closeTimeOut">Optional time out for closing the client connection.</param>
        /// <param name="keepAlive">Optional spacing between keep alives for proactive disconnection detection. If null is provided, no keep alives will be sent.</param>
        /// <param name="logger"><see cref="ILogger"/> for the client.</param>
        public WebSocketClient(WebSocket socket, string url, RequestHandler requestHandler, TimeSpan? closeTimeOut = null, TimeSpan? keepAlive = null, ILogger logger = null)
            : base(url, requestHandler, closeTimeOut, keepAlive, logger)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        }

        internal override StreamingTransport CreateStreamingTransport(IDuplexPipe application)
        {
            return new WebSocketTransport(_socket, application, Logger);
        }
    }
}
