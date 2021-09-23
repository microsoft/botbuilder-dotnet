// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Microsoft.Bot.Streaming.Transport.WebSockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    /// <summary>
    /// The <see cref="StreamingConnection"/> to be used by legacy bots.
    /// </summary>
    [Obsolete("Use `WebSocketStreamingConnection` instead.", false)]
    public sealed class LegacyStreamingConnection : StreamingConnection, IDisposable
    {
        private readonly WebSocket _socket;
        private readonly string _pipeName;
        private readonly ILogger _logger;

        private IStreamingTransportServer _server;
        private bool _serverIsConnected;
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyStreamingConnection"/> class that uses web sockets.
        /// </summary>
        /// <param name="socket">The <see cref="WebSocket"/> instance to use for legacy streaming connection.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        public LegacyStreamingConnection(WebSocket socket, ILogger logger)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyStreamingConnection"/> class that uses named pipes.
        /// </summary>
        /// <param name="pipeName">The name of the named pipe.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        public LegacyStreamingConnection(string pipeName, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
            {
                throw new ArgumentNullException(nameof(pipeName));
            }

            _pipeName = pipeName;
            _logger = logger ?? NullLogger.Instance;
        }

        /// <inheritdoc />
        public override async Task ListenAsync(RequestHandler requestHandler, CancellationToken cancellationToken = default)
        {
            _server = CreateStreamingTransportServer(requestHandler);
            _serverIsConnected = true;
            _server.Disconnected += Server_Disconnected;

            await _server.StartAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async Task<ReceiveResponse> SendStreamingRequestAsync(StreamingRequest request, CancellationToken cancellationToken = default)
        {
            if (!_serverIsConnected)
            {
                throw new InvalidOperationException("Error while attempting to send: Streaming transport is disconnected.");
            }

            return await _server.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to catch all exceptions while disconnecting.")]
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        if (_server != null)
                        {
                            _server.Disconnected -= Server_Disconnected;

                            if (_server is WebSocketServer webSocketServer)
                            {
                                webSocketServer.Disconnect();
                            }
                            else if (_server is NamedPipeServer namedPipeServer)
                            {
                                namedPipeServer.Disconnect();
                            }

                            if (_server is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to gracefully disconnect server while tearing down streaming connection.");
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        private void Server_Disconnected(object sender, DisconnectedEventArgs e)
        {
            _serverIsConnected = false;
        }

        private IStreamingTransportServer CreateStreamingTransportServer(RequestHandler requestHandler)
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
