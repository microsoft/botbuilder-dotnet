using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol.Payloads;
using Microsoft.Bot.Protocol.Utilities;

namespace Microsoft.Bot.Protocol.Payloads
{
    public class PayloadAssemblerManager
    {
        private readonly Func<Guid, ReceiveRequest, Task> _onReceiveRequest;
        private readonly Func<Guid, ReceiveResponse, Task> _onReceiveResponse;
        private readonly IStreamManager _streamManager;
        private readonly Dictionary<Guid, PayloadAssembler> _activeAssemblers;

        public PayloadAssemblerManager(
            IStreamManager streamManager,
            Func<Guid, ReceiveRequest, Task> onReceiveRequest,
            Func<Guid, ReceiveResponse, Task> onReceiveResponse)
        {
            _onReceiveRequest = onReceiveRequest;
            _onReceiveResponse = onReceiveResponse;
            _activeAssemblers = new Dictionary<Guid, PayloadAssembler>();
            _streamManager = streamManager;
        }

        public Stream GetPayloadStream(Header header)
        {
            if(IsStreamPayload(header))
            {
                return _streamManager.GetPayloadStream(header);
            }
            else if (!_activeAssemblers.TryGetValue(header.Id, out PayloadAssembler assembler))
            {
                // a new requestId has come in, start a new task to process it as it is received
                assembler = CreatePayloadAssembler(header);
                if (assembler != null)
                {
                    _activeAssemblers.Add(header.Id, assembler);
                }

                return assembler?.GetPayloadStream();
            }

            return null;
        }

        public void OnReceive(Header header, Stream contentStream, int contentLength)
        {
            if (IsStreamPayload(header))
            {
                _streamManager.OnReceive(header, contentStream, contentLength);
            }
            else
            {
                if (_activeAssemblers.TryGetValue(header.Id, out PayloadAssembler assembler))
                {
                    assembler.OnReceive(header, contentStream, contentLength);
                }

                // remove them when we are done
                if (header.End)
                {
                    _activeAssemblers.Remove(header.Id);
                }

                // ignore unknown header ids
            }
        }

        private bool IsStreamPayload(Header header)
        {
            return header.Type == PayloadTypes.Stream;
        }

        private PayloadAssembler CreatePayloadAssembler(Header header)
        {
            switch (header.Type)
            {
                case PayloadTypes.Request:
                    return new ReceiveRequestAssembler(header, _streamManager, _onReceiveRequest);
                case PayloadTypes.Response:
                    return new ReceiveResponseAssembler(header, _streamManager, _onReceiveResponse);
            }
            return null;
        }
    }
}
