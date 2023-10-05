// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.PayloadTransport;
using Microsoft.Bot.Streaming.Utilities;

#if SIGNASSEMBLY
[assembly: InternalsVisibleTo("Microsoft.Bot.Streaming.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
#else
[assembly: InternalsVisibleTo("Microsoft.Bot.Streaming.Tests")]
#endif

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
            if (response.StatusCode == (int)HttpStatusCode.Accepted)
            {
                return;
            }

            // we received the response to something, signal it
            await _requestManager.SignalResponseAsync(id, response).ConfigureAwait(false);
        }

        private void OnCancelStream(IAssembler contentStreamAssembler)
        {
            Background.Run(() => _sendOperations.SendCancelStreamAsync(contentStreamAssembler.Id));
        }
    }
}
