// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    internal class PayloadStreamAssembler : PayloadAssembler
    {
        private readonly IStreamManager _streamManager;

        public PayloadStreamAssembler(IStreamManager streamManager, Guid id)
            : base(id)
        {
            _streamManager = streamManager ?? new StreamManager();
        }

        public PayloadStreamAssembler(IStreamManager streamManager, Guid id, string type, int? length)
            : this(streamManager, id)
        {
            ContentType = type;
            ContentLength = length;
        }

        public int? ContentLength { get; set; }

        public string ContentType { get; set; } = string.Empty;

        public override Stream CreateStreamFromPayload() => new PayloadStream(this);

        public override void OnReceive(Header header, Stream stream, int contentLength)
        {
            base.OnReceive(header, stream, contentLength);

            if (End)
            {
                ((PayloadStream)stream).DoneProducing();
            }
        }

        public override void Close() => _streamManager.CloseStream(Id);
    }
}
