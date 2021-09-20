// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    /// <summary>
    /// A factory that can create an instance of <see cref="StreamingConnection"/>.
    /// </summary>
    public interface IStreamingConnectionFactory
    {
        /// <summary>
        /// Creates a <see cref="StreamingConnection"/> that uses web sockets.
        /// </summary>
        /// <param name="httpContext"><see cref="HttpContext"/> instance on which to accept the web socket.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        /// <returns>A <see cref="StreamingConnection"/> that uses web sockets.</returns>
        StreamingConnection CreateWebSocketConnection(HttpContext httpContext, ILogger logger);

        /// <summary>
        /// Creates a <see cref="StreamingConnection"/> that uses web sockets.
        /// </summary>
        /// <param name="socket">The <see cref="WebSocket"/> instance to use for legacy streaming connection.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        /// <returns>A <see cref="StreamingConnection"/> that uses web sockets.</returns>
        StreamingConnection CreateLegacyWebSocketConnection(WebSocket socket, ILogger logger);

        /// <summary>
        /// Creates a <see cref="StreamingConnection"/> that uses named pipes.
        /// </summary>
        /// <param name="pipeName">The name of the named pipe.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        /// <returns>A <see cref="StreamingConnection"/> that uses named pipes.</returns>
        StreamingConnection CreateLegacyNamedPipeConnection(string pipeName, ILogger logger);
    }
}
