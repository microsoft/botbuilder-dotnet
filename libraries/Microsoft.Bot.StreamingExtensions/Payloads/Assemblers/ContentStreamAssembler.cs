// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    internal class ContentStreamAssembler : PayloadAssembler
    {
        private readonly IStreamManager _streamManager;

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

        public int? ContentLength { get; set; }

        public string ContentType { get; set; }

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
