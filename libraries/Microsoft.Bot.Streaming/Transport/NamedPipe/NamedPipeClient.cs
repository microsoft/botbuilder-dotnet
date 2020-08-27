// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.PayloadTransport;
using Microsoft.Bot.Streaming.Utilities;

namespace Microsoft.Bot.Streaming.Transport.NamedPipes
{
    /// <summary>
    /// An implementation of <see cref="IStreamingTransportClient"/> for use with Named Pipes.
    /// </summary>
    public class NamedPipeClient : IStreamingTransportClient
    {
        private readonly string _baseName;
        private readonly RequestHandler _requestHandler;
        private readonly IPayloadSender _sender;
        private readonly IPayloadReceiver _receiver;
        private readonly RequestManager _requestManager;
        private readonly ProtocolAdapter _protocolAdapter;
        private readonly bool _autoReconnect;
        private readonly object _syncLock = new object();
        private bool _isDisconnecting;

        // To detect redundant calls to dispose
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeClient"/> class.
        /// Throws <see cref="ArgumentNullException"/> if baseName is null, empty, or whitespace.
        /// </summary>
        /// <param name="baseName">The named pipe to connect to.</param>
        /// <param name="requestHandler">Optional <see cref="RequestHandler"/> to process incoming messages received by this client.</param>
        /// <param name="autoReconnect">Optional setting to determine if the client sould attempt to reconnect
        /// automatically on disconnection events. Defaults to true.
        /// </param>
        public NamedPipeClient(string baseName, RequestHandler requestHandler = null, bool autoReconnect = true)
        {
            if (string.IsNullOrWhiteSpace(baseName))
            {
                throw new ArgumentNullException(nameof(baseName));
            }

            _baseName = baseName;
            _requestHandler = requestHandler;
            _autoReconnect = autoReconnect;

            _requestManager = new RequestManager();

            _sender = new PayloadSender();
            _sender.Disconnected += OnConnectionDisconnected;
            _receiver = new PayloadReceiver();
            _receiver.Disconnected += OnConnectionDisconnected;

            _protocolAdapter = new ProtocolAdapter(_requestHandler, _requestManager, _sender, _receiver);
        }

        /// <summary>
        /// An event to be fired when the underlying transport is disconnected. Any application communicating with this client should subscribe to this event.
        /// </summary>
        public event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// Gets a value indicating whether or not this client is currently connected.
        /// </summary>
        /// <returns>
        /// True if this client is connected and ready to send and receive messages, otherwise false.
        /// </returns>
        /// <value>
        /// A boolean value indicating whether or not this client is currently connected.
        /// </value>
        public bool IsConnected => IncomingConnected && OutgoingConnected;

        /// <summary>
        /// Gets a value indicating whether the NamedPipeClient has an incoming pipe connection.
        /// </summary>
        /// <value>
        /// A boolean value indicating whether or not this client is currently connected to an incoming pipe.
        /// </value>
        public bool IncomingConnected => _receiver.IsConnected;

        /// <summary>
        /// Gets a value indicating whether the NamedPipeClient has an outgoing pipe connection.
        /// </summary>
        /// <value>
        /// A boolean value indicating whether or not this client is currently connected to an outgoing pipe.
        /// </value>
        public bool OutgoingConnected => _receiver.IsConnected;

        /// <summary>
        /// Establish a connection with no custom headers.
        /// </summary>
        /// <returns>A <see cref="Task"/> that will not resolve until the client stops listening for incoming messages.</returns>
        public Task ConnectAsync() => ConnectAsync(null);

        /// <summary>
        /// Establish a connection with optional custom headers.
        /// </summary>
        /// <param name="requestHeaders">An optional <see cref="IDictionary{TKey, TValue}"/> of string header names and string header values to include when sending the
        /// initial request to establish this connection.
        /// </param>
        /// <returns>A <see cref="Task"/> that will not resolve until the client stops listening for incoming messages.</returns>
        public async Task ConnectAsync(IDictionary<string, string> requestHeaders)
        {
            var outgoingPipeName = _baseName + NamedPipeTransport.ServerIncomingPath;
            var outgoing = new NamedPipeClientStream(".", outgoingPipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            await outgoing.ConnectAsync().ConfigureAwait(false);

            var incomingPipeName = _baseName + NamedPipeTransport.ServerOutgoingPath;
            var incoming = new NamedPipeClientStream(".", incomingPipeName, PipeDirection.In, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            await incoming.ConnectAsync().ConfigureAwait(false);

#pragma warning disable CA2000 // Dispose objects before losing scope

            // We don't dispose the websocket, since NamedPipeTransport is now
            // the owner of the web socket.
            _sender.Connect(new NamedPipeTransport(outgoing));
            _receiver.Connect(new NamedPipeTransport(incoming));

#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        /// <summary>
        /// Task used to send data over this client connection.
        /// Throws <see cref="InvalidOperationException"/> if called when the client is disconnected.
        /// Throws <see cref="ArgumentNullException"/> if message is null.
        /// </summary>
        /// <param name="message">The <see cref="StreamingRequest"/> to send.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> used to signal this operation should be cancelled.</param>
        /// <returns>A <see cref="Task"/> that will produce an instance of <see cref="ReceiveResponse"/> on completion of the send operation.</returns>
        public async Task<ReceiveResponse> SendAsync(StreamingRequest message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (!_sender.IsConnected || !_receiver.IsConnected)
            {
                throw new InvalidOperationException("The client is not connected.");
            }

            return await _protocolAdapter.SendRequestAsync(message, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Method used to disconnect this client.
        /// </summary>
        public void Disconnect()
        {
            _sender?.Disconnect();
            _receiver?.Disconnect();
        }

        /// <summary>
        /// Disconnects the client and releases any related objects owned by the class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                Disconnect();

                if (_sender is IDisposable disposableSender)
                {
                    disposableSender?.Dispose();
                }

                if (_receiver is IDisposable disposableReceiver)
                {
                    disposableReceiver?.Dispose();
                }
            }

            _disposed = true;
        }

        private void OnConnectionDisconnected(object sender, EventArgs e)
        {
            bool doDisconnect = false;
            if (!_isDisconnecting)
            {
                lock (_syncLock)
                {
                    if (!_isDisconnecting)
                    {
                        _isDisconnecting = true;
                        doDisconnect = true;
                    }
                }
            }

            if (doDisconnect)
            {
                try
                {
                    if (_sender.IsConnected)
                    {
                        _sender.Disconnect();
                    }

                    if (_receiver.IsConnected)
                    {
                        _receiver.Disconnect();
                    }

                    Disconnected?.Invoke(this, DisconnectedEventArgs.Empty);

                    if (_autoReconnect)
                    {
                        // Try to rerun the client connection
                        Background.Run(ConnectAsync);
                    }
                }
                finally
                {
                    lock (_syncLock)
                    {
                        _isDisconnecting = false;
                    }
                }
            }
        }
    }
}
