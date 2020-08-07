// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Transport.WebSockets
{
    /// <summary>
    /// An implementation of <see cref="ITransportSender"/> and <see cref="ITransportReceiver"/> for use with a WebSocket transport.
    /// </summary>
    public class WebSocketTransport : ITransportSender, ITransportReceiver
    {
        private readonly WebSocket _socket;

        // To detect redundant calls to dispose
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketTransport"/> class.
        /// </summary>
        /// <param name="socket">The WebSocket to bind this transport to.</param>
        public WebSocketTransport(WebSocket socket)
        {
            _socket = socket;
        }

        /// <inheritdoc/>
        public bool IsConnected => _socket.State == WebSocketState.Open;

        /// <inheritdoc/>
        public void Close()
        {
            if (_socket.State == WebSocketState.Open)
            {
                try
                {
                    Task.WaitAll(_socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closed by the WebSocketTransport",
                        CancellationToken.None));
                }
#pragma warning disable CA1031 // Do not catch general exception types (ignore exceptions while the socket is being closed)
                catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    // Any exception thrown here will be caused by the socket already being closed,
                    // which is the state we want to put it in by calling this method, which
                    // means we don't care if it was already closed and threw an exception
                    // when we tried to close it again.
                }
            }
        }

        /// <summary>
        /// Disposes the object and releases any related objects owned by the class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
        {
            try
            {
                if (_socket != null)
                {
                    var memory = new ArraySegment<byte>(buffer, offset, count);
                    var result = await _socket.ReceiveAsync(memory, CancellationToken.None).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket closed", CancellationToken.None).ConfigureAwait(false);
                        if (_socket.State == WebSocketState.Closed)
                        {
                            _socket.Dispose();
                        }
                    }

                    return result.Count;
                }
            }
            catch (Exception ex)
            {
                // Exceptions of the three types below will also have set the socket's state to closed, which fires an
                // event consumers of this class are subscribed to and have handling around. Any other exception needs to
                // be thrown to cause a non-transport-connectivity failure.
                if (!(ex is ObjectDisposedException) && !(ex is OperationCanceledException) && !(ex is WebSocketException))
                {
                    throw;
                }
            }

            return 0;
        }

        /// <inheritdoc/>
        public async Task<int> SendAsync(byte[] buffer, int offset, int count)
        {
            try
            {
                if (_socket != null)
                {
                    var memory = new ArraySegment<byte>(buffer, offset, count);
                    await _socket.SendAsync(memory, WebSocketMessageType.Binary, true, CancellationToken.None).ConfigureAwait(false);
                    return count;
                }
            }
            catch (Exception ex)
            {
                // Exceptions of the three types below will also have set the socket's state to closed, which fires an
                // event consumers of this class are subscribed to and have handling around. Any other exception needs to
                // be thrown to cause a non-transport-connectivity failure.
                if (!(ex is ObjectDisposedException) && !(ex is OperationCanceledException) && !(ex is WebSocketException) && !(ex is IOException))
                {
                    throw;
                }
            }

            return 0;
        }
        
        /// <summary>
        /// Disposes objected used by the class.
        /// </summary>
        /// <param name="disposing">A Boolean that indicates whether the method call comes from a Dispose method (its value is true) or from a finalizer (its value is false).</param>
        /// <remarks>
        /// The disposing parameter should be false when called from a finalizer, and true when called from the IDisposable.Dispose method.
        /// In other words, it is true when deterministically called and false when non-deterministically called.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed objects owned by the class here.
                _socket?.Dispose();
            }

            _disposed = true;
        }
    }
}
