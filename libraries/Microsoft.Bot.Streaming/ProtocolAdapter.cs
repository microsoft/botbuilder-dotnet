// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.PayloadTransport;
using Microsoft.Bot.Streaming.Utilities;

namespace Microsoft.Bot.Streaming
{
    internal class ProtocolAdapter
    {
        private readonly RequestHandler _requestHandler;
        private readonly object _handlerContext;
        private readonly IPayloadSender _payloadSender;
        private readonly IPayloadReceiver _payloadReceiver;
        private readonly IRequestManager _requestManager;
        private readonly SendOperations _sendOperations;
        private readonly IStreamManager _streamManager;
        private readonly PayloadAssemblerManager _assemblerManager;

        public ProtocolAdapter(RequestHandler requestHandler, IRequestManager requestManager, IPayloadSender payloadSender, IPayloadReceiver payloadReceiver, object handlerContext = null)
        {
            _requestHandler = requestHandler;
            _handlerContext = handlerContext;
            _requestManager = requestManager;
            _payloadSender = payloadSender;
            _payloadReceiver = payloadReceiver;

            _sendOperations = new SendOperations(_payloadSender);
            _streamManager = new StreamManager(OnCancelStream);
            _assemblerManager = new PayloadAssemblerManager(_streamManager, OnReceiveRequestAsync, OnReceiveResponseAsync);

            _payloadReceiver.Subscribe(_assemblerManager.GetPayloadStream, _assemblerManager.OnReceive);
        }

        public async Task<ReceiveResponse> SendRequestAsync(StreamingRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestId = Guid.NewGuid();
            var responseTask = _requestManager.GetResponseAsync(requestId, cancellationToken);
            var requestTask = _sendOperations.SendRequestAsync(requestId, request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await Task.WhenAll(requestTask, responseTask).ConfigureAwait(false);

            return await responseTask.ConfigureAwait(false);
        }

        private async Task OnReceiveRequestAsync(Guid id, ReceiveRequest request)
        {
            // request is done, we can handle it
            if (_requestHandler != null)
            {
                var response = await _requestHandler.ProcessRequestAsync(request, null, context: _handlerContext).ConfigureAwait(false);

                if (response != null)
                {
                    await _sendOperations.SendResponseAsync(id, response).ConfigureAwait(false);
                }
            }
        }

        private async Task OnReceiveResponseAsync(Guid id, ReceiveResponse response)
        {
            // we received the response to something, signal it
            await _requestManager.SignalResponseAsync(id, response).ConfigureAwait(false);
        }

        private void OnCancelStream(IAssembler contentStreamAssembler)
        {
            Background.Run(() => _sendOperations.SendCancelStreamAsync(contentStreamAssembler.Id));
        }
    }
}
