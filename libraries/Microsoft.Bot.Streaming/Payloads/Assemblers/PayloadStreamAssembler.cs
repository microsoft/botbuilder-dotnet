// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Newtonsoft.Json;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// An <see cref="IAssembler"/> specific to payload streams.
    /// </summary>
    public class PayloadStreamAssembler : IAssembler
    {
        private object _syncLock = new object();
        private readonly IStreamManager _streamManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadStreamAssembler"/> class.
        /// </summary>
        /// <param name="streamManager">The <see cref="IStreamManager"/> managing the stream being assembled.</param>
        /// <param name="id">The ID of this instance.</param>
        public PayloadStreamAssembler(IStreamManager streamManager, Guid id)
        {
            _streamManager = streamManager ?? new StreamManager();
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadStreamAssembler"/> class.
        /// </summary>
        /// <param name="streamManager">The <see cref="IStreamManager"/> managing the stream being assembled.</param>
        /// <param name="id">The ID of this instance.</param>
        /// <param name="type">The type of the content contained within the stream.</param>
        /// <param name="length">The length of the content contained within the stream.</param>
        public PayloadStreamAssembler(IStreamManager streamManager, Guid id, string type, int? length)
            : this(streamManager, id)
        {
            ContentType = type;
            ContentLength = length;
        }

        /// <summary>
        /// Gets or sets the length of the content contained in the payload.
        /// </summary>
        /// <value>
        /// A positive value if a fixed length exists, otherwise 0.
        /// </value>
        public int? ContentLength { get; set; }

        /// <summary>
        /// Gets or sets the type of the content contained in the payload.
        /// </summary>
        /// <value>
        /// A content type as defined by the Type field of <see cref="Header"/>.
        /// </value>
        public string ContentType { get; set; } = string.Empty;

        /// <inheritdoc/>
        public Guid Id { get; private set; }

        /// <inheritdoc/>
        public bool End { get; private set; }

        private Stream Stream { get; set; }

        /// <inheritdoc/>
        public Stream CreateStreamFromPayload() => new PayloadStream(this);

        /// <inheritdoc/>
        public Stream GetPayloadAsStream()
        {
            lock (_syncLock)
            {
                if (Stream == null)
                {
                    Stream = CreateStreamFromPayload();
                }
            }

            return Stream;
        }

        /// <inheritdoc/>
        public void OnReceive(Header header, Stream stream, int contentLength)
        {
            if (header.End)
            {
                End = true;
                ((PayloadStream)stream).DoneProducing();
            }
        }

        /// <inheritdoc/>
        public void Close() => _streamManager.CloseStream(Id);
    }
}
