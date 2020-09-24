// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.PayloadTransport;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// The <see cref="PayloadDisassembler"/> used for <see cref="StreamingRequest"/> payloads.
    /// </summary>
    public class RequestDisassembler : PayloadDisassembler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestDisassembler"/> class.
        /// </summary>
        /// <param name="sender">The <see cref="PayloadSender"/> to send the disassembled data to.</param>
        /// <param name="id">The ID of this disassembler.</param>
        /// <param name="request">The request to be disassembled.</param>
        public RequestDisassembler(IPayloadSender sender, Guid id, StreamingRequest request)
            : base(sender, id)
        {
            Request = request;
        }

        /// <summary>
        /// Gets the <see cref="StreamingRequest"/> to be disassembled.
        /// </summary>
        /// <value>
        /// The <see cref="StreamingRequest"/> to be disassembled.
        /// </value>
        public StreamingRequest Request { get; private set; }

        /// <inheritdoc/>
        public override char Type => PayloadTypes.Request;

        /// <inheritdoc/>
        public override Task<StreamWrapper> GetStreamAsync()
        {
            var payload = new RequestPayload()
            {
                Verb = Request.Verb,
                Path = Request.Path,
            };

            if (Request.Streams != null)
            {
                payload.Streams = new List<StreamDescription>();
                foreach (var contentStream in Request.Streams)
                {
                    var description = GetStreamDescription(contentStream);

                    payload.Streams.Add(description);
                }
            }

            Serialize(payload, out MemoryStream memoryStream, out int streamLength);

            return Task.FromResult(new StreamWrapper()
            {
                Stream = memoryStream,
                StreamLength = streamLength,
            });
        }
    }
}
