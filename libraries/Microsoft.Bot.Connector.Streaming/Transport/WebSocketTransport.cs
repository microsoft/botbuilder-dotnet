// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Connector.Streaming.Transport
{
    internal class WebSocketTransport
    {
        private readonly IDuplexPipe _application;
        private readonly ILogger _logger;

        private volatile bool _aborted;

        public WebSocketTransport(IDuplexPipe application, ILogger logger)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _logger = logger ?? NullLogger.Instance;
        }

        public async Task ConnectAsync(HttpContext context, CancellationToken cancellationToken)
        {
            using (var ws = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false))
            {
                Log.SocketOpened(_logger);

                try
                {
                    await ProcessSocketAsync(ws, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    Log.SocketClosed(_logger);
                }
            }
        }

        public async Task ConnectAsync(string url, IDictionary<string, string> requestHeaders = null, CancellationToken cancellationToken = default)
        {
            using (var ws = new ClientWebSocket())
            {
                Log.SocketOpened(_logger);

                try
                {
                    if (requestHeaders != null)
                    {
                        foreach (var key in requestHeaders.Keys)
                        {
                            ws.Options.SetRequestHeader(key, requestHeaders[key]);
                        }
                    }

                    await ws.ConnectAsync(new Uri(url), cancellationToken).ConfigureAwait(false);

                    await ProcessSocketAsync(ws, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    Log.SocketClosed(_logger);
                }
            }
        }

        internal async Task ProcessSocketAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            // Begin sending and receiving. Receiving must be started first because ExecuteAsync enables SendAsync.
            var receiving = StartReceivingAsync(socket, cancellationToken);
            var sending = StartSendingAsync(socket);

            // Wait for send or receive to complete
            var trigger = await Task.WhenAny(receiving, sending).ConfigureAwait(false);

            if (trigger == receiving)
            {
                Log.WaitingForSend(_logger);

                // We're waiting for the application to finish and there are 2 things it could be doing
                // 1. Waiting for application data
                // 2. Waiting for a websocket send to complete

                // Cancel the application so that ReadAsync yields
                _application.Input.CancelPendingRead();

                using (var delayCts = new CancellationTokenSource())
                {
                    // TODO: flow this timeout to allow draining
                    var resultTask = await Task.WhenAny(sending, Task.Delay(TimeSpan.FromSeconds(1), delayCts.Token)).ConfigureAwait(false);

                    if (resultTask != sending)
                    {
                        // We timed out so now we're in ungraceful shutdown mode
                        Log.CloseTimedOut(_logger);

                        // Abort the websocket if we're stuck in a pending send to the client
                        _aborted = true;

                        socket.Abort();
                    }
                    else
                    {
                        delayCts.Cancel();
                    }
                }
            }
            else
            {
                Log.WaitingForClose(_logger);

                // We're waiting on the websocket to close and there are 2 things it could be doing
                // 1. Waiting for websocket data
                // 2. Waiting on a flush to complete (backpressure being applied)

                using (var delayCts = new CancellationTokenSource())
                {
                    var resultTask = await Task.WhenAny(receiving, Task.Delay(TimeSpan.FromSeconds(1), delayCts.Token)).ConfigureAwait(false);

                    if (resultTask != receiving)
                    {
                        // Abort the websocket if we're stuck in a pending receive from the client
                        _aborted = true;

                        socket.Abort();

                        // Cancel any pending flush so that we can quit
                        _application.Output.CancelPendingFlush();
                    }
                    else
                    {
                        delayCts.Cancel();
                    }
                }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to catch all exceptions in the message loop.")]
        private async Task StartReceivingAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Do a 0 byte read so that idle connections don't allocate a buffer when waiting for a read
                    var result = await socket.ReceiveAsync(GetArraySegment(Memory<byte>.Empty), cancellationToken).ConfigureAwait(false);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }

                    var memory = _application.Output.GetMemory();

                    var arraySegment = GetArraySegment(memory);
                    var receiveResult = await socket.ReceiveAsync(arraySegment, cancellationToken).ConfigureAwait(false);

                    // Need to check again for netcoreapp3.0 and later because a close can happen between a 0-byte read and the actual read
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }

                    Log.MessageReceived(_logger, receiveResult.MessageType, receiveResult.Count, receiveResult.EndOfMessage);

                    _application.Output.Advance(receiveResult.Count);
                    var flushResult = await _application.Output.FlushAsync().ConfigureAwait(false);

                    // We canceled in the middle of applying back pressure
                    // or if the consumer is done
                    if (flushResult.IsCanceled || flushResult.IsCompleted)
                    {
                        break;
                    }
                }
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                // Client has closed the WebSocket connection without completing the close handshake
                Log.ClosedPrematurely(_logger, ex);
            }
            catch (OperationCanceledException)
            {
                // Ignore aborts, don't treat them like transport errors
            }
            catch (Exception ex)
            {
                if (!_aborted && !cancellationToken.IsCancellationRequested)
                {
                    await _application.Output.CompleteAsync(ex).ConfigureAwait(false);
                    Log.TransportError(_logger, ex);
                }
            }
            finally
            {
                // We're done writing.
                await _application.Output.CompleteAsync().ConfigureAwait(false);
                Log.ReceivingCompleted(_logger);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to catch all exceptions in the message loop.")]
        private async Task StartSendingAsync(WebSocket socket)
        {
            Exception error = null;

            try
            {
                while (true)
                {
                    var result = await _application.Input.ReadAsync().ConfigureAwait(false);
                    var buffer = result.Buffer;

                    // Get a frame from the application
                    try
                    {
                        if (result.IsCanceled)
                        {
                            break;
                        }

                        if (!buffer.IsEmpty)
                        {
                            try
                            {
                                Log.SendPayload(_logger, buffer.Length);

                                if (WebSocketCanSend(socket))
                                {
                                    await socket.SendAsync(buffer, WebSocketMessageType.Binary, CancellationToken.None).ConfigureAwait(false);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (!_aborted)
                                {
                                    Log.ErrorWritingFrame(_logger, ex);
                                }

                                break;
                            }
                        }
                        else if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    finally
                    {
                        _application.Input.AdvanceTo(buffer.End);
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                // Send the close frame before calling into user code
                if (WebSocketCanSend(socket))
                {
                    try
                    {
                        // We're done sending, send the close frame to the client if the websocket is still open
                        await socket.CloseOutputAsync(error != null ? WebSocketCloseStatus.InternalServerError : WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Log.ClosingWebSocketFailed(_logger, ex);
                    }
                }

                Log.SendingCompleted(_logger);
                await _application.Input.CompleteAsync().ConfigureAwait(false);
            }
        }

        private bool WebSocketCanSend(WebSocket ws)
        {
            return !(ws.State == WebSocketState.Aborted ||
                   ws.State == WebSocketState.Closed ||
                   ws.State == WebSocketState.CloseSent);
        }

        /// <summary>
        /// Log messages for <see cref="WebSocketTransport"/>.
        /// </summary>
        /// <remarks>
        /// Messages implementred using <see cref="LoggerMessage.Define(LogLevel, EventId, string)"/> to maximize performance.
        /// For more information, see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/loggermessage?view=aspnetcore-5.0.
        /// </remarks>
        private static class Log
        {
            private static readonly Action<ILogger, Exception> _socketOpened =
                LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(SocketOpened)), "Socket transport connection opened.");

            private static readonly Action<ILogger, Exception> _socketClosed =
                LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(SocketClosed)), "Socket transport connection closed.");

            private static readonly Action<ILogger, Exception> _waitingForSend =
                LoggerMessage.Define(LogLevel.Debug, new EventId(3, nameof(WaitingForSend)), "Waiting for the application to finish sending data.");

            private static readonly Action<ILogger, Exception> _waitingForClose =
                LoggerMessage.Define(LogLevel.Debug, new EventId(4, nameof(WaitingForClose)), "Waiting for the client to close the socket.");

            private static readonly Action<ILogger, Exception> _closeTimedOut =
                LoggerMessage.Define(LogLevel.Debug, new EventId(5, nameof(CloseTimedOut)), "Timed out waiting for client to send the close frame, aborting the connection.");

            private static readonly Action<ILogger, WebSocketMessageType, int, bool, Exception> _messageReceived =
                LoggerMessage.Define<WebSocketMessageType, int, bool>(LogLevel.Trace, new EventId(6, nameof(MessageReceived)), "Message received. Type: {MessageType}, size: {Size}, EndOfMessage: {EndOfMessage}.");

            private static readonly Action<ILogger, long, Exception> _sendPayload =
                LoggerMessage.Define<long>(LogLevel.Trace, new EventId(7, nameof(SendPayload)), "Sending payload: {Size} bytes.");

            private static readonly Action<ILogger, Exception> _errorWritingFrame =
                LoggerMessage.Define(LogLevel.Debug, new EventId(8, nameof(ErrorWritingFrame)), "Error writing frame.");

            private static readonly Action<ILogger, Exception> _closedPrematurely =
                LoggerMessage.Define(LogLevel.Debug, new EventId(9, nameof(ClosedPrematurely)), "Socket connection closed prematurely.");

            private static readonly Action<ILogger, Exception> _closingWebSocketFailed =
                LoggerMessage.Define(LogLevel.Debug, new EventId(10, nameof(ClosingWebSocketFailed)), "Closing webSocket failed.");

            private static readonly Action<ILogger, Exception> _sendingCompleted =
                LoggerMessage.Define(LogLevel.Information, new EventId(11, nameof(SendingCompleted)), "Socket transport sending task completed.");

            private static readonly Action<ILogger, Exception> _receivingCompleted =
                LoggerMessage.Define(LogLevel.Information, new EventId(12, nameof(ReceivingCompleted)), "Socket transport receiving task completed.");

            private static readonly Action<ILogger, Exception> _transportError =
                LoggerMessage.Define(LogLevel.Error, new EventId(13, nameof(TransportError)), "Transport error deteted.");

            public static void SocketOpened(ILogger logger) => _socketOpened(logger, null);

            public static void SocketClosed(ILogger logger) => _socketClosed(logger, null);

            public static void WaitingForSend(ILogger logger) => _waitingForSend(logger, null);

            public static void WaitingForClose(ILogger logger) => _waitingForClose(logger, null);

            public static void CloseTimedOut(ILogger logger) => _closeTimedOut(logger, null);

            public static void MessageReceived(ILogger logger, WebSocketMessageType type, int size, bool endOfMessage) => _messageReceived(logger, type, size, endOfMessage, null);

            public static void SendPayload(ILogger logger, long size) => _sendPayload(logger, size, null);

            public static void ErrorWritingFrame(ILogger logger, Exception ex) => _errorWritingFrame(logger, ex);

            public static void ClosedPrematurely(ILogger logger, Exception ex) => _closedPrematurely(logger, ex);

            public static void ClosingWebSocketFailed(ILogger logger, Exception ex) => _closingWebSocketFailed(logger, ex);

            public static void SendingCompleted(ILogger logger) => _sendingCompleted(logger, null);

            public static void ReceivingCompleted(ILogger logger) => _receivingCompleted(logger, null);

            public static void TransportError(ILogger logger, Exception ex) => _transportError(logger, ex);
        }
    }
}
