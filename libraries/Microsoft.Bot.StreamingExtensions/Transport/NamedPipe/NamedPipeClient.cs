using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.Bot.StreamingExtensions.PayloadTransport;
using Microsoft.Bot.StreamingExtensions.Transport;
using Microsoft.Bot.StreamingExtensions.Utilities;

namespace Microsoft.Bot.StreamingExtensions.Transport.NamedPipes
{
    public class NamedPipeClient : IStreamingTransportClient
    {
        private readonly string _baseName;
        private readonly RequestHandler _requestHandler;
        private readonly IPayloadSender _sender;
        private readonly IPayloadReceiver _receiver;
        private readonly RequestManager _requestManager;
        private readonly ProtocolAdapter _protocolAdapter;
        private readonly bool _autoReconnect;
        private object _syncLock = new object();
        private bool _isDisconnecting = false;

        public NamedPipeClient(string baseName, RequestHandler requestHandler = null, bool autoReconnect = true)
        {
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

        public event DisconnectedEventHandler Disconnected;

        /// <inheritdoc />
        public Task ConnectAsync() => ConnectAsync(null);

        /// <inheritdoc />
        public async Task ConnectAsync(IDictionary<string, string> requestHeaders)
        {
            var outgoingPipeName = _baseName + NamedPipeTransport.ServerIncomingPath;
            var outgoing = new NamedPipeClientStream(".", outgoingPipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            await outgoing.ConnectAsync().ConfigureAwait(false);

            var incomingPipeName = _baseName + NamedPipeTransport.ServerOutgoingPath;
            var incoming = new NamedPipeClientStream(".", incomingPipeName, PipeDirection.In, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            await incoming.ConnectAsync().ConfigureAwait(false);

            _sender.Connect(new NamedPipeTransport(outgoing));
            _receiver.Connect(new NamedPipeTransport(incoming));
        }

        public bool IncomingConnected
        {
            get { return _receiver.IsConnected; }
        }

        public bool OutgoingConnected
        {
            get { return _receiver.IsConnected; }
        }

        public bool IsConnected => IncomingConnected && OutgoingConnected;

        public async Task<ReceiveResponse> SendAsync(Request message, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _protocolAdapter.SendRequestAsync(message, cancellationToken).ConfigureAwait(false);
        }

        public void Disconnect()
        {
            _sender.Disconnect();
            _receiver.Disconnect();
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

        public void Dispose()
        {
            Disconnect();
        }
    }
}
