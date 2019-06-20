using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.Bot.StreamingExtensions.PayloadTransport;
using Microsoft.Bot.StreamingExtensions.Transport;

namespace Microsoft.Bot.StreamingExtensions.Transport.WebSockets
{
    public class WebSocketClient : IStreamingTransportClient
    {
        private readonly string _url;
        private readonly RequestHandler _requestHandler;
        private readonly RequestManager _requestManager;
        private readonly ProtocolAdapter _protocolAdapter;
        private readonly IPayloadSender _sender;
        private readonly IPayloadReceiver _receiver;
        private bool _isDisconnecting = false;

        // UTC time of the last send on this client. Made available so we can clean up idle clients.
        public DateTime LastMessageSendTime { get; private set; }

        // Whether the client thinks it is currently connected.
        public bool IsConnected { get; private set; }

        public event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// Create the web socket client
        /// </summary>
        /// <param name="url">URL to talk to</param>
        /// <param name="requestHandler">A handler for the requests</param>
        public WebSocketClient(string url, RequestHandler requestHandler = null, object handlerContext = null)
        {
            _url = url;
            _requestHandler = requestHandler;
            _requestManager = new RequestManager();

            _sender = new PayloadSender();
            _receiver = new PayloadReceiver();

            _protocolAdapter = new ProtocolAdapter(_requestHandler, _requestManager, _sender, _receiver, handlerContext);

            IsConnected = false;
        }

        /// <inheritdoc />
        public Task ConnectAsync() => ConnectAsync(null);

        /// <inheritdoc />
        public async Task ConnectAsync(IDictionary<string, string> requestHeaders = null)
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

            await clientWebSocket.ConnectAsync(new Uri(_url), CancellationToken.None).ConfigureAwait(false);
            var socketTransport = new WebSocketTransport(clientWebSocket);

            // Listen for disconnected events.
            _sender.Disconnected += OnConnectionDisconnected;
            _receiver.Disconnected += OnConnectionDisconnected;

            _sender.Connect(socketTransport);
            _receiver.Connect(socketTransport);

            IsConnected = true;
        }

        public void Connect()
        {
            ConnectAsync().Wait();
        }

        public Task<ReceiveResponse> SendAsync(Request message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_sender.IsConnected || !_receiver.IsConnected)
            {
                throw new InvalidOperationException("The client is not connected.");
            }

            LastMessageSendTime = DateTime.UtcNow;
            return _protocolAdapter.SendRequestAsync(message, cancellationToken);
        }

        public void Disconnect()
        {
            _sender.Disconnect();
            _receiver.Disconnect();

            _sender.Disconnected -= OnConnectionDisconnected;
            _receiver.Disconnected -= OnConnectionDisconnected;

            IsConnected = false;
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

        public void Dispose()
        {
            Disconnect();
        }
    }
}
