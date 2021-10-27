﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.PayloadTransport;

namespace Microsoft.Bot.Streaming.Transport.WebSockets
{
    /// <summary>
    /// A client for use with the Bot Framework Protocol V3 with Streaming Extensions and an underlying WebSocket transport.
    /// </summary>
    public class WebSocketClient : IStreamingTransportClient
    {
        private readonly string _url;
        private readonly RequestHandler _requestHandler;
        private readonly RequestManager _requestManager;
        private readonly ProtocolAdapter _protocolAdapter;
        private readonly IPayloadSender _sender;
        private readonly IPayloadReceiver _receiver;
        private bool _isDisconnecting;

        // To detect redundant calls to dispose
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketClient"/> class.
        /// Throws <see cref="ArgumentNullException"/> if URL is null, empty, or whitespace.
        /// </summary>
        /// <param name="url">The URL of the remote server to connect to.</param>
        /// <param name="requestHandler">Optional <see cref="RequestHandler"/> to process incoming messages received by this server.</param>
        /// <param name="handlerContext">Optional context for the <see cref="RequestHandler"/> to operate within.</param>
        public WebSocketClient(string url, RequestHandler requestHandler = null, object handlerContext = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            _url = url;
            _requestHandler = requestHandler;
            _requestManager = new RequestManager();

            _sender = new PayloadSender();
            _receiver = new PayloadReceiver();

            _protocolAdapter = new ProtocolAdapter(_requestHandler, _requestManager, _sender, _receiver, handlerContext);

            IsConnected = false;
        }

        /// <summary>
        /// An event to be fired when the underlying transport is disconnected. Any application communicating with this client should subscribe to this event.
        /// </summary>
        public event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// Gets the UTC time of the last send on this client. Made available for use when cleaning up idle clients.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> representing the UTC time of the last send on this client.
        /// </value>
        public DateTime LastMessageSendTime { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not this client is currently connected.
        /// </summary>
        /// <returns>
        /// True if this client is connected and ready to send and receive messages, otherwise false.
        /// </returns>
        /// <value>
        /// A boolean value indicating whether or not this client is currently connected.
        /// </value>
        public bool IsConnected { get; private set; }

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
        public async Task ConnectAsync(IDictionary<string, string> requestHeaders = null)
        {
            await ConnectAsyncEx(requestHeaders).ConfigureAwait(false);
        }

        /// <summary>
        /// Establish a connection with optional custom headers.
        /// </summary>
        /// <param name="requestHeaders">An optional <see cref="IDictionary{TKey, TValue}"/> of string header names and string header values to include when sending the
        /// initial request to establish this connection.
        /// </param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> used to signal this operation should be cancelled.</param>
        /// <returns>A <see cref="Task"/> that will not resolve until the client stops listening for incoming messages.</returns>
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods (can't change this without breaking binary compat)
        public async Task ConnectAsyncEx(IDictionary<string, string> requestHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            if (IsConnected)
            {
                return;
            }

            var clientWebSocket = new ClientWebSocket();
            if (requestHeaders != null)
            {
                foreach (var key in requestHeaders.Keys)
                {
                    clientWebSocket.Options.SetRequestHeader(key, requestHeaders[key]);
                }
            }

            await clientWebSocket.ConnectAsync(new Uri(_url), cancellationToken).ConfigureAwait(false);

#pragma warning disable CA2000 // Dispose objects before losing scope

            // We don't dispose the websocket, since WebSocketTransport is now
            // the owner of the web socket.
            var socketTransport = new WebSocketTransport(clientWebSocket);
#pragma warning restore CA2000 // Dispose objects before losing scope

            // Listen for disconnected events.
            _sender.Disconnected += OnConnectionDisconnected;
            _receiver.Disconnected += OnConnectionDisconnected;

            _sender.Connect(socketTransport);
            _receiver.Connect(socketTransport);

            IsConnected = true;
        }

        /// <summary>
        /// Task used to send data over this client connection.
        /// Throws <see cref="InvalidOperationException"/> if called when the client is disconnected.
        /// Throws <see cref="ArgumentNullException"/> if message is null.
        /// </summary>
        /// <param name="message">The <see cref="StreamingRequest"/> to send.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> used to signal this operation should be cancelled.</param>
        /// <returns>A <see cref="Task"/> that will produce an instance of <see cref="ReceiveResponse"/> on completion of the send operation.</returns>
        public Task<ReceiveResponse> SendAsync(StreamingRequest message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (!_sender.IsConnected || !_receiver.IsConnected)
            {
                throw new InvalidOperationException("The client is not connected.");
            }

            LastMessageSendTime = DateTime.UtcNow;
            return _protocolAdapter.SendRequestAsync(message, cancellationToken);
        }

        /// <summary>
        /// Method used to disconnect this client.
        /// </summary>
        public void Disconnect()
        {
            _sender.Disconnect();
            _receiver.Disconnect();

            _sender.Disconnected -= OnConnectionDisconnected;
            _receiver.Disconnected -= OnConnectionDisconnected;

            IsConnected = false;
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
        /// Establish a connection with injected web socket for more control in tests.
        /// </summary>
        /// <param name="socket">A <see cref="WebSocket"/> for the client which msut already be .</param>
        /// <returns>A <see cref="Task"/> that will not resolve until the client stops listening for incoming messages.</returns>
        internal Task ConnectInternalAsync(WebSocket socket)
        {
            if (IsConnected)
            {
                return Task.CompletedTask;
            }

            // We don't dispose the websocket, since WebSocketTransport is now
            // the owner of the web socket.
#pragma warning disable CA2000 // Dispose objects before losing scope
            var socketTransport = new WebSocketTransport(socket);
#pragma warning restore CA2000 // Dispose objects before losing scope

            // Listen for disconnected events.
            _sender.Disconnected += OnConnectionDisconnected;
            _receiver.Disconnected += OnConnectionDisconnected;

            _sender.Connect(socketTransport);
            _receiver.Connect(socketTransport);

            IsConnected = true;

            return Task.CompletedTask;
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
            }

            _disposed = true;
        }

        private void OnConnectionDisconnected(object sender, EventArgs e)
        {
            if (!_isDisconnecting)
            {
                _isDisconnecting = true;

                if (sender == _sender)
                {
                    _receiver.Disconnect();
                }

                if (sender == _receiver)
                {
                    _sender.Disconnect();
                }

                IsConnected = false;

                Disconnected?.Invoke(this, DisconnectedEventArgs.Empty);

                _isDisconnecting = false;
            }
        }
    }
}
