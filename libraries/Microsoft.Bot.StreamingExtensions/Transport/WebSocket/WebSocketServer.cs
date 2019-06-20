using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol.Payloads;
using Microsoft.Bot.Protocol.PayloadTransport;
using Microsoft.Bot.Protocol.Transport;

namespace Microsoft.Bot.Protocol.WebSockets
{
    public class WebSocketServer : IStreamingTransportServer
    {
        private readonly RequestHandler _requestHandler;
        private readonly RequestManager _requestManager;
        private readonly ProtocolAdapter _protocolAdapter;
        private readonly IPayloadSender _sender;
        private readonly IPayloadReceiver _receiver;
        private WebSocketTransport _websocketTransport;
        private TaskCompletionSource<string> _closedSignal;
        private bool _isDisconnecting = false;

        public WebSocketServer(WebSocket socket, RequestHandler requestHandler)
        {
            _websocketTransport = new WebSocketTransport(socket);
            _requestHandler = requestHandler;

            _requestManager = new RequestManager();

            _sender = new PayloadSender();
            _sender.Disconnected += OnConnectionDisconnected;
            _receiver = new PayloadReceiver();
            _receiver.Disconnected += OnConnectionDisconnected;

            _protocolAdapter = new ProtocolAdapter(_requestHandler, _requestManager, _sender, _receiver);
        }

        public event DisconnectedEventHandler Disconnected;

        public Task StartAsync()
        {
            _closedSignal = new TaskCompletionSource<string>();
            _sender.Connect(_websocketTransport);
            _receiver.Connect(_websocketTransport);
            return _closedSignal.Task;
        }

        public bool IsConnected
        {
            get { return _sender.IsConnected && _receiver.IsConnected; }
        }

        public Task<ReceiveResponse> SendAsync(Request request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_sender.IsConnected || !_receiver.IsConnected)
            {
                throw new InvalidOperationException("The server is not connected.");
            }

            return _protocolAdapter.SendRequestAsync(request, cancellationToken);
        }

        public void Disconnect()
        {
            _sender.Disconnect();
            _receiver.Disconnect();
        }

        private void OnConnectionDisconnected(object sender, EventArgs e)
        {
            if (!_isDisconnecting)
            {
                _isDisconnecting = true;

                if (_closedSignal != null)
                {
                    _closedSignal.SetResult("close");
                    _closedSignal = null;
                }

                if (sender == _sender)
                {
                    _receiver.Disconnect();
                }

                if (sender == _receiver)
                {
                    _sender.Disconnect();
                }
                
                Disconnected?.Invoke(this, DisconnectedEventArgs.Empty);

                _isDisconnecting = false;
            }
        }
    }
}
