// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Session;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Bot.Streaming;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    /// <summary>
    /// A streaming based connection that can listen for incoming requests and send them to a <see cref="RequestHandler"/>, 
    /// and can also send requests to the other end of the connection.
    /// </summary>
    public abstract class StreamingConnection : IDisposable
    {
        private readonly TaskCompletionSource<bool> _sessionInitializedTask = new TaskCompletionSource<bool>();

        private StreamingTransport _transport;
        private TransportHandler _application;
        private StreamingSession _session;

        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingConnection"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/> for the connection.</param>
        protected StreamingConnection(ILogger logger)
        {
            Logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Gets the <see cref="ILogger"/> instance for the streaming connection.
        /// </summary>
        /// <value>A <see cref="ILogger"/> for the streaming connection.</value>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets a value indicating whether this is currently connected.
        /// </summary>
        /// <value>
        /// True if this is currently connected, otherwise false.
        /// </value>
        protected bool IsConnected { get; private set; } = false;

        /// <summary>
        /// Sends a streaming request through the connection.
        /// </summary>
        /// <param name="request"><see cref="StreamingRequest"/> to be sent.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel the send process.</param>
        /// <returns>The <see cref="ReceiveResponse"/> returned from the client.</returns>
        public virtual async Task<ReceiveResponse> SendStreamingRequestAsync(StreamingRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // This request could come fast while the session, transport and application are still being set up.
            // Wait for the session to signal that application and transport started before using the session.
            await _sessionInitializedTask.Task.ConfigureAwait(false);

            if (_session == null)
            {
                throw new InvalidOperationException("Cannot send streaming request since the session is not set up.");
            }

            try
            {
                return await _session.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (TimeoutException ex)
            {
                var timeoutMessage = $"The connection to the client has been disconnected, and the request has timed out after waiting {TaskExtensions.DefaultTimeout.Seconds} seconds for a response.";
                if (IsConnected)
                {
                    timeoutMessage = $"The request sent to the client has timed out after waiting {TaskExtensions.DefaultTimeout.Seconds} seconds for a response.";
                }

                throw new OperationCanceledException(timeoutMessage, ex, cancellationToken);
            }
        }

        /// <summary>
        /// Opens the <see cref="StreamingConnection"/> and listens for incoming requests, which will
        /// be assembled and sent to the provided <see cref="RequestHandler"/>.
        /// </summary>
        /// <param name="requestHandler"><see cref="RequestHandler"/> to which incoming requests will be sent.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that signals the need to stop the connection. 
        /// Once the token is cancelled, the connection will be gracefully shut down, finishing pending sends and receives.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task ListenAsync(RequestHandler requestHandler, CancellationToken cancellationToken = default)
        {
            _transport?.Dispose();
            _application?.Dispose();

            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            var duplexPipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

            // Create transport and application
            _transport = CreateStreamingTransport(duplexPipePair.Application);
            _application = new TransportHandler(duplexPipePair.Transport, Logger);

            // Create session
            _session = new StreamingSession(requestHandler, _application, Logger, cancellationToken);

            // Start transport and application
            var transportTask = _transport.ConnectAsync((connected) => IsConnected = connected, cancellationToken);
            var applicationTask = _application.ListenAsync(cancellationToken);

            var tasks = new List<Task> { transportTask, applicationTask };

            // Signal that session is ready to be used
            _sessionInitializedTask.SetResult(true);

            // Let application and transport run
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        internal abstract StreamingTransport CreateStreamingTransport(IDuplexPipe application);

        /// <summary>
        /// Disposes managed and unmanaged resources of the underlying <see cref="StreamingConnection"/>.
        /// </summary>
        /// <param name="disposing">Whether we are disposing managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _transport?.Dispose();
                    _application?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
