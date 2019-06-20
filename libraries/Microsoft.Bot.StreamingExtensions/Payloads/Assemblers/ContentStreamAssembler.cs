using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol.Payloads;
using Microsoft.Bot.Protocol.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Bot.Protocol.Payloads
{
    public class ContentStreamAssembler : PayloadAssembler
    {
        private readonly IStreamManager _streamManager;

        public int? ContentLength { get; set; }

        public string ContentType { get; set; }

        public ContentStreamAssembler(IStreamManager streamManager, Guid id)
            : base(id)
        {
            ContentType = null;
            ContentLength = null;
            _streamManager = streamManager;
        }

        public ContentStreamAssembler(IStreamManager streamManager, Guid id, string type, int? length)
            : this(streamManager, id)
        {
            ContentType = type;
            ContentLength = length;
        }

        public override Stream CreatePayloadStream()
        {
            return new ConcurrentStream(this);
        }

        public override void OnReceive(Header header, Stream stream, int contentLength)
        {
            base.OnReceive(header, stream, contentLength);

            if (End)
            {
                ((ConcurrentStream)stream).DoneProducing();
            }
        }

        public override void Close()
        {
            _streamManager.CloseStream(Id);
        }
    }
}
