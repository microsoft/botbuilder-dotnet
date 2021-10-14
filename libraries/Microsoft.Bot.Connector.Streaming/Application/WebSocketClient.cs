// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Session;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static Microsoft.Bot.Connector.Streaming.Transport.DuplexPipe;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    /// <summary>
    /// Web socket client.
    /// </summary>
    public class WebSocketClient : IStreamingTransportClient
    {
        private readonly string _url;
        private readonly RequestHandler _requestHandler;
        private readonly ILogger _logger;
        private readonly TimeSpan _closeTimeout;
        private readonly TimeSpan? _keepAlive;

        private CancellationTokenSource _disconnectCts;
        private StreamingSession _session;
        private TransportHandler _transportHandler;
        private DuplexPipePair _duplexPipePair;
        private volatile bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketClient"/> class.
        /// </summary>
        /// <param name="url">Url to connect to.</param>
        /// <param name="requestHandler">Request handler that will receive incoming requests to this client instance.</param>
        /// <param name="closeTimeOut">Optional time out for closing the client connection.</param>
        /// <param name="keepAlive">Optional spacing between keep alives for proactive disconnection detection. If null is provided, no keep alives will be sent.</param>
        /// <param name="logger"><see cref="ILogger"/> for the client.</param>
        public WebSocketClient(string url, RequestHandler requestHandler, TimeSpan? closeTimeOut = null, TimeSpan? keepAlive = null, ILogger logger = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            _url = url;
            _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
            _logger = logger ?? NullLogger.Instance;
            _closeTimeout = closeTimeOut ?? TimeSpan.FromSeconds(15);
            _keepAlive = keepAlive;
        }

        internal WebSocketClient(RequestHandler requestHandler, TimeSpan? closeTimeOut = null, TimeSpan? keepAlive = null, ILogger logger = null)
        {
            _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
            _logger = logger ?? NullLogger.Instance;
            _closeTimeout = closeTimeOut ?? TimeSpan.FromSeconds(15);
            _keepAlive = keepAlive;
        }

        /// <inheritdoc/>
        public event DisconnectedEventHandler Disconnected;

        /// <inheritdoc/>
        public bool IsConnected { get; set; } = false;

        /// <inheritdoc/>
        public async Task ConnectAsync()
        {
            await ConnectAsync(new Dictionary<string, string>(), CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(IDictionary<string, string> requestHeaders)
        {
            await ConnectAsync(requestHeaders, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Establishes the connection.
        /// </summary>
        /// <param name="requestHeaders">Request headers.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for the client connection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ConnectAsync(IDictionary<string, string> requestHeaders, CancellationToken cancellationToken)
        {
            await ConnectInternalAsync(
                connectFunc: transport => transport.ConnectAsync(_url, requestHeaders, CancellationToken.None),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ReceiveResponse> SendAsync(StreamingRequest message, CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            if (_session == null)
            {
                throw new InvalidOperationException("Session not established. Call ConnectAsync() in order to send requests through this client.");
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return await _session.SendRequestAsync(message, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Disconnect()
        {
            CheckDisposed();
            DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Disconnects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DisconnectAsync()
        {
            CheckDisposed();
            await _transportHandler.StopAsync().ConfigureAwait(false);
            IsConnected = false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal async Task ConnectInternalAsync(WebSocket clientSocket, CancellationToken cancellationToken)
        {
            await ConnectInternalAsync(
                connectFunc: transport => transport.ProcessSocketAsync(clientSocket, CancellationToken.None),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        internal async Task ConnectInternalAsync(Func<WebSocketTransport, Task> connectFunc, CancellationToken cancellationToken)
        {
            CheckDisposed();

            TimerAwaitable timer = null;
            Task timerTask = null;

            try
            {
                // Pipes
                _duplexPipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

                // Transport
                var transport = new WebSocketTransport(_duplexPipePair.Application, _logger);

                // Application
                _transportHandler = new TransportHandler(_duplexPipePair.Transport, _logger);

                // Session
                _session = new StreamingSession(_requestHandler, _transportHandler, _logger);

                // Set up cancellation
                _disconnectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                // Start transport and application
                var transportTask = connectFunc(transport);
                var applicationTask = _transportHandler.ListenAsync(_disconnectCts.Token);
                var combinedTask = Task.WhenAll(transportTask, applicationTask);

                Log.ClientStarted(_logger, _url ?? string.Empty);

                // Periodic task: keep alive
                // Disposed with `timer.Stop()` in the finally block below
                if (_keepAlive.HasValue)
                {
                    timer = new TimerAwaitable(_keepAlive.Value, _keepAlive.Value);
                    timerTask = TimerLoopAsync(timer);
                }

                // We are connected!
                IsConnected = true;

                // Block until transport or application ends.
                await combinedTask.ConfigureAwait(false);

                // Signal that we're done
                _disconnectCts.Cancel();
                Log.ClientTransportApplicationCompleted(_logger, _url);
            }
            finally
            {
                timer?.Stop();

                if (timerTask != null)
                {
                    await timerTask.ConfigureAwait(false);
                }
            }

            Log.ClientCompleted(_logger, _url ?? string.Empty);
        }

        internal async Task TimerLoopAsync(TimerAwaitable timer)
        {
            timer.Start();

            using (timer)
            {
                // await returns True until `timer.Stop()` is called in the `finally` block of `ReceiveLoop`
                while (await timer)
                {
                    try
                    {
                        // Ping server
                        var response = await _session.SendRequestAsync(StreamingRequest.CreateGet("/api/version"), _disconnectCts.Token).ConfigureAwait(false);

                        if (!IsSuccessResponse(response))
                        {
                            Log.ClientKeepAliveFail(_logger, _url, response.StatusCode);

                            IsConnected = false;

                            Disconnected?.Invoke(this, new DisconnectedEventArgs() { Reason = $"Received failure from server heartbeat: {response.StatusCode}." });
                        }
                        else
                        {
                            Log.ClientKeepAliveSucceed(_logger, _url);
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        Log.ClientKeepAliveFail(_logger, _url, 0, e);
                        IsConnected = false;
                        Disconnected?.Invoke(this, new DisconnectedEventArgs() { Reason = $"Received failure from server heartbeat: {e}." });
                    }
                }
            }
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
                try
                {
                    Disconnect();
                    _disconnectCts.Cancel();
                }
                finally
                {
                    _transportHandler.Dispose();
                    _disconnectCts.Dispose();
                }
            }

            _disposed = true;
        }

        private static bool IsSuccessResponse(ReceiveResponse response)
        {
            return response != null && response.StatusCode >= 200 && response.StatusCode <= 299;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private class Log
        {
            private static readonly Action<ILogger, string, Exception> _clientStarted =
                LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(ClientStarted)), "WebSocket client connected to {string}.");

            private static readonly Action<ILogger, string, Exception> _clientCompleted =
                LoggerMessage.Define<string>(LogLevel.Information, new EventId(2, nameof(ClientKeepAliveSucceed)), "WebSocket client connection to {string} closed.");

            private static readonly Action<ILogger, string, Exception> _clientKeepAliveSucceed =
                LoggerMessage.Define<string>(LogLevel.Debug, new EventId(3, nameof(ClientStarted)), "WebSocket client heartbeat to {string} succeeded.");

            private static readonly Action<ILogger, string, int, Exception> _clientKeepAliveFail =
                LoggerMessage.Define<string, int>(LogLevel.Error, new EventId(4, nameof(ClientKeepAliveFail)), "WebSocket client heartbeat to {string} failed with status code {int}.");

            private static readonly Action<ILogger, string, Exception> _clientTransportApplicationCompleted =
                LoggerMessage.Define<string>(LogLevel.Debug, new EventId(5, nameof(ClientTransportApplicationCompleted)), "WebSocket client heartbeat to {string} completed transport and application tasks.");

            public static void ClientStarted(ILogger logger, string url) => _clientStarted(logger, url ?? string.Empty, null);

            public static void ClientCompleted(ILogger logger, string url) => _clientCompleted(logger, url ?? string.Empty, null);

            public static void ClientKeepAliveSucceed(ILogger logger, string url) => _clientKeepAliveSucceed(logger, url ?? string.Empty, null);

            public static void ClientKeepAliveFail(ILogger logger, string url, int statusCode = 0, Exception e = null) => _clientKeepAliveFail(logger, url ?? string.Empty, statusCode, e);

            public static void ClientTransportApplicationCompleted(ILogger logger, string url) => _clientTransportApplicationCompleted(logger, url ?? string.Empty, null);
        }
    }
}
