// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.PayloadTransport;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// The <see cref="PayloadDisassembler"/> used used by Cancel requests. 
    /// </summary>
    public class CancelDisassembler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CancelDisassembler"/> class.
        /// </summary>
        /// <param name="sender">The <see cref="PayloadSender"/> this Cancel request will be sent by.</param>
        /// <param name="id">The ID of the <see cref="PayloadStream"/> to cancel.</param>
        /// <param name="type">The type of the <see cref="PayloadStream"/> that is being cancelled.</param>
        public CancelDisassembler(IPayloadSender sender, Guid id, char type)
        {
            Sender = sender;
            Id = id;
            Type = type;
        }

        private IPayloadSender Sender { get; set; }

        private Guid Id { get; set; }

        private char Type { get; set; }

        /// <summary>
        /// A task that initiates the process of disassembling the request and signals the <see cref="PayloadSender"/> to begin sending.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods (can't change this without breaking binary compat)
        public Task Disassemble()
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            var header = new Header()
            {
                Type = Type,
                Id = Id,
                PayloadLength = 0,
                End = true,
            };

            Sender.SendPayload(header, null, true, null);

            return Task.CompletedTask;
        }
    }
}
