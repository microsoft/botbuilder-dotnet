// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Connector.Streaming.Transport
{
    internal abstract class StreamingTransport : IDisposable
    {
        private readonly IDuplexPipe _application;

        private volatile bool _aborted;

        protected StreamingTransport(IDuplexPipe application, ILogger logger)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            Logger = logger ?? NullLogger.Instance;
        }

        protected ILogger Logger { get; }

        public abstract Task ConnectAsync(CancellationToken cancellationToken);

        public abstract Task ConnectAsync(string url, IDictionary<string, string> requestHeaders = null, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected async Task ProcessAsync(CancellationToken cancellationToken)
        {
            // Begin sending and receiving. Receiving must be started first because ExecuteAsync enables SendAsync.
            var receiving = StartReceivingAsync(cancellationToken);
            var sending = StartSendingAsync();

            // Wait for send or receive to complete
            var trigger = await Task.WhenAny(receiving, sending).ConfigureAwait(false);

            if (trigger == receiving)
            {
                Log.WaitingForSend(Logger);

                // We're waiting for the application to finish, and there are 2 things it could be doing:
                // 1. Waiting for application data
                // 2. Waiting for a transport send to complete

                // Cancel the application so that ReadAsync yields
                _application.Input.CancelPendingRead();

                using (var delayCts = new CancellationTokenSource())
                {
                    var resultTask = await Task.WhenAny(sending, Task.Delay(TimeSpan.FromSeconds(1), delayCts.Token)).ConfigureAwait(false);

                    if (resultTask != sending)
                    {
                        // We timed out, so now we're in ungraceful shutdown mode
                        Log.CloseTimedOut(Logger);
                        
                        // Abort the transport if we're stuck in a pending send to the client
                        _aborted = true;
                        Abort();
                    }
                    else
                    {
                        delayCts.Cancel();
                    }
                }
            }
            else
            {
                Log.WaitingForClose(Logger);

                // We're waiting on the transport to close, and there are 2 things it could be doing:
                // 1. Waiting for transport data
                // 2. Waiting on a flush to complete (back pressure being applied)

                using (var delayCts = new CancellationTokenSource())
                {
                    var resultTask = await Task.WhenAny(receiving, Task.Delay(TimeSpan.FromSeconds(1), delayCts.Token)).ConfigureAwait(false);

                    if (resultTask != receiving)
                    {
                        // Abort the transport if we're stuck in a pending receive from the client
                        _aborted = true;
                        Abort();
                        
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

        /// <summary>
        /// Receive stream from transport into the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer where incoming stream will be received into.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel the receive process.</param>
        /// <returns>The actual number of bytes read if receive was successful; -1 if receive encountered an error.</returns>
        protected abstract Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);

        /// <summary>
        /// Send stream to transport from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer that contains outgoing stream.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel the send process.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        protected abstract Task SendAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken);

        /// <summary>
        /// Whether the transport can send outgoing stream.
        /// </summary>
        /// <returns>true if the transport can send outgoing stream; false otherwise.</returns>
        protected abstract bool CanSend();

        /// <summary>
        /// Close the transport connection if it is still open.
        /// </summary>
        /// <param name="error">The transport error, if any; null if there were no errors.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel the close process.</param>
        /// <returns>A task that represents the asynchronous close operation.</returns>
        protected abstract Task CloseOutputAsync(Exception error, CancellationToken cancellationToken);

        protected abstract void Abort();

        /// <summary>
        /// Disposes managed and unmanaged resources of the underlying <see cref="StreamingTransport"/>.
        /// </summary>
        /// <param name="disposing">Whether we are disposing managed resources.</param>
        protected abstract void Dispose(bool disposing);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to catch all exceptions in the message loop.")]
        private async Task StartReceivingAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Do a 0 byte read so that idle connections don't allocate a buffer when waiting for a read
                    var result = await ReceiveAsync(Memory<byte>.Empty, cancellationToken).ConfigureAwait(false);

                    if (result == -1)
                    {
                        return;
                    }

                    var memory = _application.Output.GetMemory();
                    var receiveResult = await ReceiveAsync(memory, cancellationToken).ConfigureAwait(false);

                    // Need to check again for netcoreapp3.0 and later, because a close can happen between a 0-byte read and the actual read
                    if (receiveResult == -1)
                    {
                        return;
                    }

                    Log.MessageReceived(Logger, receiveResult);

                    _application.Output.Advance(receiveResult);
                    var flushResult = await _application.Output.FlushAsync(cancellationToken).ConfigureAwait(false);

                    // We canceled in the middle of applying back pressure, or, if the consumer is done
                    if (flushResult.IsCanceled || flushResult.IsCompleted)
                    {
                        break;
                    }
                }
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
                    Log.TransportError(Logger, ex);
                }
            }
            finally
            {
                // We're done receiving.
                await _application.Output.CompleteAsync().ConfigureAwait(false);
                Log.ReceivingCompleted(Logger);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to catch all exceptions in the message loop.")]
        private async Task StartSendingAsync()
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
                                Log.SendPayload(Logger, buffer.Length);

                                if (CanSend())
                                {
                                    await SendAsync(buffer, CancellationToken.None).ConfigureAwait(false);
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
                                    Log.ErrorWritingFrame(Logger, ex);
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
                if (CanSend())
                {
                    try
                    {
                        // We're done sending, send the close frame to the client if the transport is still open
                        await CloseOutputAsync(error, CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Log.ClosingTransportFailed(Logger, ex);
                    }
                }

                Log.SendingCompleted(Logger);
                await _application.Input.CompleteAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Log messages for <see cref="StreamingTransport"/>.
        /// </summary>
        /// <remarks>
        /// Messages implemented using <see cref="LoggerMessage.Define(LogLevel, EventId, string)"/> to maximize performance.
        /// For more information, see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/loggermessage?view=aspnetcore-5.0.
        /// </remarks>
        private static class Log
        {
            private static readonly Action<ILogger, Exception> _waitingForSend = LoggerMessage.Define(
                LogLevel.Debug, new EventId(1, nameof(WaitingForSend)), "Waiting for the application to finish sending data.");

            private static readonly Action<ILogger, Exception> _waitingForClose = LoggerMessage.Define(
                LogLevel.Debug, new EventId(2, nameof(WaitingForClose)), "Waiting for the client to close the transport.");

            private static readonly Action<ILogger, Exception> _closeTimedOut = LoggerMessage.Define(
                LogLevel.Debug, new EventId(3, nameof(CloseTimedOut)), "Timed out waiting for client to send the close frame, aborting the connection.");

            private static readonly Action<ILogger, int, Exception> _messageReceived = LoggerMessage.Define<int>(
                LogLevel.Trace, new EventId(4, nameof(MessageReceived)), "Message received. Size: {Size}.");

            private static readonly Action<ILogger, long, Exception> _sendPayload = LoggerMessage.Define<long>(
                LogLevel.Trace, new EventId(5, nameof(SendPayload)), "Sending payload: {Size} bytes.");

            private static readonly Action<ILogger, Exception> _errorWritingFrame = LoggerMessage.Define(
                LogLevel.Debug, new EventId(6, nameof(ErrorWritingFrame)), "Error writing frame.");

            private static readonly Action<ILogger, Exception> _closingTransportFailed = LoggerMessage.Define(
                LogLevel.Debug, new EventId(7, nameof(ClosingTransportFailed)), "Closing streaming transport failed.");

            private static readonly Action<ILogger, Exception> _sendingCompleted = LoggerMessage.Define(
                LogLevel.Information, new EventId(8, nameof(SendingCompleted)), "Streaming transport sending task completed.");

            private static readonly Action<ILogger, Exception> _receivingCompleted = LoggerMessage.Define(
                LogLevel.Information, new EventId(9, nameof(ReceivingCompleted)), "Streaming transport receiving task completed.");

            private static readonly Action<ILogger, Exception> _transportError = LoggerMessage.Define(
                LogLevel.Error, new EventId(10, nameof(TransportError)), "Streaming transport error detected.");

            public static void WaitingForSend(ILogger logger) => _waitingForSend(logger, null);

            public static void WaitingForClose(ILogger logger) => _waitingForClose(logger, null);

            public static void CloseTimedOut(ILogger logger) => _closeTimedOut(logger, null);

            public static void MessageReceived(ILogger logger, int size) => _messageReceived(logger, size, null);

            public static void SendPayload(ILogger logger, long size) => _sendPayload(logger, size, null);

            public static void ErrorWritingFrame(ILogger logger, Exception ex) => _errorWritingFrame(logger, ex);

            public static void ClosingTransportFailed(ILogger logger, Exception ex) => _closingTransportFailed(logger, ex);

            public static void SendingCompleted(ILogger logger) => _sendingCompleted(logger, null);

            public static void ReceivingCompleted(ILogger logger) => _receivingCompleted(logger, null);

            public static void TransportError(ILogger logger, Exception ex) => _transportError(logger, ex);
        }
    }
}
