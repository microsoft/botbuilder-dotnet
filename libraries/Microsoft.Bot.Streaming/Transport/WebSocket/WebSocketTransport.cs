// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Transport.WebSockets
{
    public class WebSocketTransport : ITransportSender, ITransportReceiver
    {
        private readonly WebSocket _socket;

        public WebSocketTransport(WebSocket socket)
        {
            _socket = socket;
        }

        public bool IsConnected => _socket.State == WebSocketState.Open;

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
                catch (Exception)
                {
                    // Any exception thrown here will be caused by the socket already being closed,
                    // which is the state we want to put it in by calling this method, which
                    // means we don't care if it was already closed and threw an exception
                    // when we tried to close it again.
                }
            }
        }

        public void Dispose()
        {
        }

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
                        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket closed", CancellationToken.None);
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
    }
}
