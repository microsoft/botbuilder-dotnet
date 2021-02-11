// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.PayloadTransport;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// The <see cref="PayloadDisassembler"/> used for <see cref="ResponseMessageStream"/> payloads.
    /// </summary>
    public class ResponseMessageStreamDisassembler : PayloadDisassembler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseMessageStreamDisassembler"/> class.
        /// </summary>
        /// <param name="sender">The <see cref="PayloadSender"/> to send the disassembled data to.</param>
        /// <param name="contentStream">The <see cref="ResponseMessageStream"/> to be disassembled.</param>
        public ResponseMessageStreamDisassembler(IPayloadSender sender, ResponseMessageStream contentStream)
            : base(sender, contentStream.Id)
        {
            ContentStream = contentStream;
        }

        /// <summary>
        /// Gets the <see cref="ResponseMessageStream"/> to be disassembled.
        /// </summary>
        /// <value>
        /// The <see cref="ResponseMessageStream"/> to be disassembled.
        /// </value>
        public ResponseMessageStream ContentStream { get; private set; }

        /// <inheritdoc/>
        public override char Type => PayloadTypes.Stream;

        /// <inheritdoc/>
        public override async Task<StreamWrapper> GetStreamAsync()
        {
            var stream = await ContentStream.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var description = GetStreamDescription(ContentStream);

            return new StreamWrapper()
            {
                Stream = stream,
                StreamLength = description.Length,
            };
        }
    }
}
