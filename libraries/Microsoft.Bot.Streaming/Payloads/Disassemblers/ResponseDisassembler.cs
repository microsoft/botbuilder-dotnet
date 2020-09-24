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
    /// The <see cref="PayloadDisassembler"/> used for <see cref="StreamingResponse"/> payloads.
    /// </summary>
    public class ResponseDisassembler : PayloadDisassembler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseDisassembler"/> class.
        /// </summary>
        /// <param name="sender">The <see cref="PayloadSender"/> to send the disassembled data to.</param>
        /// <param name="id">The ID of this disassembler.</param>
        /// <param name="response">The response to be disassembled.</param>
        public ResponseDisassembler(IPayloadSender sender, Guid id, StreamingResponse response)
           : base(sender, id)
        {
            Response = response;
        }

        /// <summary>
        /// Gets the <see cref="StreamingResponse"/> to be disassembled.
        /// </summary>
        /// <value>
        /// The <see cref="StreamingResponse"/> to be disassembled.
        /// </value>
        public StreamingResponse Response { get; private set; }

        /// <inheritdoc/>
        public override char Type => PayloadTypes.Response;

        /// <inheritdoc/>
        public override Task<StreamWrapper> GetStreamAsync()
        {
            var payload = new ResponsePayload()
            {
                StatusCode = Response.StatusCode,
            };

            if (Response.Streams != null)
            {
                payload.Streams = new List<StreamDescription>();
                foreach (var contentStream in Response.Streams)
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
