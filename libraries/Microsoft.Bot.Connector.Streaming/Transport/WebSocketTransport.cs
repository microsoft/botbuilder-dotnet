// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Streaming.Transport
{
    internal class WebSocketTransport : StreamingTransport
    {
        private readonly WebSocket _socket;
        private bool _disposedValue;

        public WebSocketTransport(WebSocket socket, IDuplexPipe application, ILogger logger)
            : base(application, logger)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        }

        public override async Task ConnectAsync(Action<bool> connectionStatusChanged = null, CancellationToken cancellationToken = default)
        {
            Log.SocketOpened(Logger);

            try
            {
                connectionStatusChanged?.Invoke(true);
                await ProcessAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                connectionStatusChanged?.Invoke(false);
                Log.SocketClosed(Logger);
            }
        }

        public override async Task ConnectAsync(string url, Action<bool> connectionStatusChanged = null, IDictionary<string, string> requestHeaders = null, CancellationToken cancellationToken = default)
        {
            Log.SocketOpened(Logger);

            try
            {
                if (_socket is ClientWebSocket clientSocket)
                {
                    if (requestHeaders != null)
                    {
                        foreach (var key in requestHeaders.Keys)
                        {
                            clientSocket.Options.SetRequestHeader(key, requestHeaders[key]);
                        }
                    }

                    await clientSocket.ConnectAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
                    connectionStatusChanged?.Invoke(true);
                    await ProcessAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {                    
                    throw new InvalidOperationException("Only client web socket can connect to server. Please instantiate the 'WebSocketTransport' with a 'ClientWebSocket' instance.");
                }
            }
            finally
            {
                connectionStatusChanged?.Invoke(false);
                Log.SocketClosed(Logger);
            }
        }

        protected override async Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _socket.ReceiveAsync(GetArraySegment(buffer), cancellationToken).ConfigureAwait(false);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return -1;
                }

                return result.Count;
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                // Client has closed the WebSocket connection without completing the close handshake
                Log.ClosedPrematurely(Logger, ex);
                return -1;
            }
        }

        protected override async Task SendAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            await _socket.SendAsync(buffer, WebSocketMessageType.Binary, cancellationToken).ConfigureAwait(false);
        }

        protected override bool CanSend()
        {
            return !(_socket.State == WebSocketState.Aborted ||
                     _socket.State == WebSocketState.Closed ||
                     _socket.State == WebSocketState.CloseSent);
        }

        protected override async Task CloseOutputAsync(Exception error, CancellationToken cancellationToken)
        {
            await _socket.CloseOutputAsync(
                    error != null ? WebSocketCloseStatus.InternalServerError : WebSocketCloseStatus.NormalClosure,
                    statusDescription: string.Empty,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        protected override void Abort()
        {
            _socket.Abort();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _socket?.Dispose();
                }

                _disposedValue = true;
            }
        }

        private static ArraySegment<byte> GetArraySegment(ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }

        /// <summary>
        /// Log messages for <see cref="WebSocketTransport"/>.
        /// </summary>
        /// <remarks>
        /// Messages implemented using <see cref="LoggerMessage.Define(LogLevel, EventId, string)"/> to maximize performance.
        /// For more information, see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/loggermessage?view=aspnetcore-5.0.
        /// </remarks>
        private static class Log
        {
            private static readonly Action<ILogger, Exception> _socketOpened = LoggerMessage.Define(
                LogLevel.Information, new EventId(1, nameof(SocketOpened)), "Socket transport connection opened.");

            private static readonly Action<ILogger, Exception> _socketClosed = LoggerMessage.Define(
                LogLevel.Information, new EventId(2, nameof(SocketClosed)), "Socket transport connection closed.");

            private static readonly Action<ILogger, Exception> _closedPrematurely = LoggerMessage.Define(
                LogLevel.Debug, new EventId(3, nameof(ClosedPrematurely)), "Socket connection closed prematurely.");

            public static void SocketOpened(ILogger logger) => _socketOpened(logger, null);

            public static void SocketClosed(ILogger logger) => _socketClosed(logger, null);

            public static void ClosedPrematurely(ILogger logger, Exception ex) => _closedPrematurely(logger, ex);
        }
    }
}
