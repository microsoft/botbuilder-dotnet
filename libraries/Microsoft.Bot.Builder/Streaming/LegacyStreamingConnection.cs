// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Microsoft.Bot.Streaming.Transport.WebSockets;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Streaming
{
    internal sealed class LegacyStreamingConnection : StreamingConnection, IDisposable
    {
        private readonly IStreamingTransportServerFactory _serverFactory;
        private readonly ILogger _logger;

        private IStreamingTransportServer _server;
        private bool _serverIsConnected;
        private bool _disposedValue;

        public LegacyStreamingConnection(IStreamingTransportServerFactory serverFactory, ILogger logger)
        {
            _serverFactory = serverFactory ?? throw new ArgumentNullException(nameof(serverFactory));
            _logger = logger;
        }

        public override async Task ListenAsync(RequestHandler requestHandler, CancellationToken cancellationToken = default)
        {
            _server = _serverFactory.Create(requestHandler);
            _serverIsConnected = true;
            _server.Disconnected += Server_Disconnected;

            await _server.StartAsync().ConfigureAwait(false);
        }

        public override async Task<ReceiveResponse> SendStreamingRequestAsync(StreamingRequest request, CancellationToken cancellationToken = default)
        {
            if (!_serverIsConnected)
            {
                throw new InvalidOperationException("Error while attempting to send: Streaming transport is disconnected.");
            }

            return await _server.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

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
    }
}
