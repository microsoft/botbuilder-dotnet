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
    public class StreamManager : IStreamManager
    {
        private readonly ConcurrentDictionary<Guid, ContentStreamAssembler> _activeAssemblers;
        private readonly Action<ContentStreamAssembler> _onCancelStream;

        public StreamManager(Action<ContentStreamAssembler> onCancelStream)
        {
            _activeAssemblers = new ConcurrentDictionary<Guid, ContentStreamAssembler>();
            _onCancelStream = onCancelStream;
        }

        public ContentStreamAssembler GetPayloadAssembler(Guid id)
        {
            if (!_activeAssemblers.TryGetValue(id, out ContentStreamAssembler assembler))
            {
                // a new id has come in, start a new task to process it
                assembler = new ContentStreamAssembler(this, id);
                if (!_activeAssemblers.TryAdd(id, assembler))
                {
                    // Don't need to dispose the assembler because it was never used
                    // Get the one that is in use
                    _activeAssemblers.TryGetValue(id, out assembler);
                }
            }
            
            return assembler;
        }

        public Stream GetPayloadStream(Header header)
        {
            var assembler = GetPayloadAssembler(header.Id);

            return assembler.GetPayloadStream();
        }

        public void OnReceive(Header header, Stream contentStream, int contentLength)
        {
            if (_activeAssemblers.TryGetValue(header.Id, out ContentStreamAssembler assembler))
            {
                assembler.OnReceive(header, contentStream, contentLength);
            }
        }

        public void CloseStream(Guid id)
        {
            if(_activeAssemblers.TryRemove(id, out ContentStreamAssembler assembler))
            {
                // decide whether to cancel it or not
                var stream = assembler.GetPayloadStream();
                if ((assembler.ContentLength.HasValue && stream.Length < assembler.ContentLength.Value) ||
                    !assembler.End)
                {
                    _onCancelStream(assembler);
                }
            }
        }
    }
}
