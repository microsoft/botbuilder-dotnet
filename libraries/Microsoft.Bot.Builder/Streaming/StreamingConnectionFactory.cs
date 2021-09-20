// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Streaming
{
    /// <inheritdoc />
    public class StreamingConnectionFactory : IStreamingConnectionFactory
    {
        /// <inheritdoc />
        public StreamingConnection CreateWebSocketConnection(HttpContext httpContext, ILogger logger)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            return new WebSocketStreamingConnection(httpContext, logger ?? NullLogger.Instance);
        }

        /// <inheritdoc />
        public StreamingConnection CreateLegacyWebSocketConnection(WebSocket socket, ILogger logger)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            return new LegacyStreamingConnection(new StreamingTransportServerFactory(socket), logger ?? NullLogger.Instance);
        }

        /// <inheritdoc />
        public StreamingConnection CreateLegacyNamedPipeConnection(string pipeName, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
            {
                throw new ArgumentNullException(nameof(pipeName));
            }

            return new LegacyStreamingConnection(new StreamingTransportServerFactory(pipeName), logger ?? NullLogger.Instance);
        }
    }
}
