// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.PayloadTransport;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// A set of tasks used for attaching one or more <see cref="PayloadDisassembler"/>s to a single <see cref="PayloadSender"/> which multiplexes data chunks from
    /// multiple disassembled payloads and sends them out over the wire via a shared <see cref="Transport.ITransportSender"/>.
    /// </summary>
    public class SendOperations
    {
        private readonly IPayloadSender _payloadSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendOperations"/> class.
        /// </summary>
        /// <param name="payloadSender">The <see cref="IPayloadSender"/> that will send the disassembled data from all of this instance's send operations.</param>
        public SendOperations(IPayloadSender payloadSender)
        {
            _payloadSender = payloadSender;
        }

        /// <summary>
        /// The send operation used to send a <see cref="StreamingRequest"/>.
        /// </summary>
        /// <param name="id">The ID to assign to the <see cref="RequestDisassembler"/> used by this operation.</param>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">A cancelation token. Unused.</param>
        /// <returns>A task representing the status of the operation.</returns>
        public async Task SendRequestAsync(Guid id, StreamingRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var disassembler = new RequestDisassembler(_payloadSender, id, request);

            await disassembler.DisassembleAsync(cancellationToken).ConfigureAwait(false);

            if (request.Streams != null)
            {
                var tasks = new List<Task>(request.Streams.Count);
                foreach (var contentStream in request.Streams)
                {
                    var contentDisassembler = new ResponseMessageStreamDisassembler(_payloadSender, contentStream);

                    tasks.Add(contentDisassembler.DisassembleAsync(cancellationToken));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The send operation used to send a <see cref="PayloadTypes.Response"/>.
        /// </summary>
        /// <param name="id">The ID to assign to the <see cref="ResponseDisassembler"/> used by this operation.</param>
        /// <param name="response">The response to send.</param>
        /// <returns>A task representing the status of the operation.</returns>
        public async Task SendResponseAsync(Guid id, StreamingResponse response)
        {
            var disassembler = new ResponseDisassembler(_payloadSender, id, response);

            await disassembler.DisassembleAsync().ConfigureAwait(false);

            if (response.Streams != null)
            {
                var tasks = new List<Task>(response.Streams.Count);
                foreach (var contentStream in response.Streams)
                {
                    var contentDisassembler = new ResponseMessageStreamDisassembler(_payloadSender, contentStream);

                    tasks.Add(contentDisassembler.DisassembleAsync());
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The send operation used to send a <see cref="PayloadTypes.CancelAll"/>.
        /// </summary>
        /// <param name="id">The ID to assign to the <see cref="CancelDisassembler"/> used by this operation.</param>
        /// <returns>A task representing the status of the operation.</returns>
        public async Task SendCancelAllAsync(Guid id)
        {
            var disassembler = new CancelDisassembler(_payloadSender, id, PayloadTypes.CancelAll);

            await disassembler.Disassemble().ConfigureAwait(false);
        }

        /// <summary>
        /// The send operation used to send a <see cref="Microsoft.Bot.Streaming.Payloads.PayloadTypes.CancelStream"/>.
        /// </summary>
        /// <param name="id">The ID to assign to the <see cref="CancelDisassembler"/> used by this operation.</param>
        /// <returns>A task representing the status of the operation.</returns>
        public async Task SendCancelStreamAsync(Guid id)
        {
            var disassembler = new CancelDisassembler(_payloadSender, id, PayloadTypes.CancelStream);

            await disassembler.Disassemble().ConfigureAwait(false);
        }
    }
}
