// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.PayloadTransport;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// PayloadDisassemblers take data payloads and break them into chunks to be sent out over the transport and reassembled on the receiving side.
    /// This allows for payload multiplexing and avoids a single large payload from blocking the transport.
    /// </summary>
    public abstract class PayloadDisassembler
    {
        private TaskCompletionSource<bool> _taskCompletionSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadDisassembler"/> class.
        /// </summary>
        /// <param name="sender">The <see cref="PayloadSender"/> used to send the disassembled payload chunks.</param>
        /// <param name="id">The ID of this disassembler.</param>
        public PayloadDisassembler(IPayloadSender sender, Guid id)
        {
            Sender = sender;
            Id = id;
            _taskCompletionSource = new TaskCompletionSource<bool>();
        }

        /// <summary>
        /// Gets the one character type of the payload this disassembler is operating on. <see cref="TransportConstants"/>.
        /// </summary>
        /// <value>
        /// The one character type of the payload this disassembler is operating on. <see cref="TransportConstants"/>.
        /// </value>
        public abstract char Type { get; }

        /// <summary>
        /// Gets or sets the <see cref="JsonSerializer"/> for use by this disassembler. Used to set custom <see cref="SerializationSettings"/>.
        /// </summary>
        /// <value>
        /// The <see cref="JsonSerializer"/> for use by this disassembler. Used to set custom <see cref="SerializationSettings"/>.
        /// </value>
        protected static JsonSerializer Serializer { get; set; } = JsonSerializer.Create(SerializationSettings.DefaultSerializationSettings);

        private IPayloadSender Sender { get; set; }

        private Stream Stream { get; set; }

        private int? StreamLength { get; set; }

        private int SendOffset { get; set; }

        private Guid Id { get; set; }

        private bool IsEnd { get; set; } = false;

        /// <summary>
        /// Gets the stream this disassembler is operating on.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public abstract Task<StreamWrapper> GetStreamAsync();

        /// <summary>
        /// Begins the process of disassembling a payload and sending the resulting chunks to the <see cref="PayloadSender"/> to dispatch over the transport.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token. Not currently used.</param>
        /// <returns>A task representing the state of the disassembly.</returns>
#pragma warning disable CA1801 // Review unused parameters
        public async Task DisassembleAsync(CancellationToken cancellationToken = default(CancellationToken))
#pragma warning restore CA1801 // Review unused parameters
        {
            var w = await GetStreamAsync().ConfigureAwait(false);

            Stream = w.Stream;
            StreamLength = w.StreamLength;
            SendOffset = 0;

            await SendAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Creates and returns the <see cref="StreamDescription"/> of the passed <see cref="ResponseMessageStream"/>.
        /// </summary>
        /// <param name="stream">The stream to create a <see cref="StreamDescription"/> for.</param>
        /// <returns>The <see cref="StreamDescription"/> of the passed in <see cref="ResponseMessageStream"/>.</returns>
        protected static StreamDescription GetStreamDescription(ResponseMessageStream stream)
        {
            var description = new StreamDescription()
            {
                Id = stream.Id.ToString("D"),
            };

            if (stream.Content.Headers.TryGetValues(HeaderNames.ContentType, out IEnumerable<string> contentType))
            {
                description.ContentType = contentType?.FirstOrDefault();
            }

            if (stream.Content.Headers.TryGetValues(HeaderNames.ContentLength, out IEnumerable<string> contentLength))
            {
                var value = contentLength?.FirstOrDefault();
                if (value != null && int.TryParse(value, out int length))
                {
                    description.Length = length;
                }
            }
            else
            {
                description.Length = (int?)stream.Content.Headers.ContentLength;
            }

            return description;
        }

        /// <summary>
        /// Serializes the item into the <see cref="MemoryStream"/> and exposes the length of the result.
        /// </summary>
        /// <typeparam name="T">The type of the item to be serialized.</typeparam>
        /// <param name="item">The item to be serialized.</param>
        /// <param name="stream">The <see cref="MemoryStream"/> to write the serialized data to.</param>
        /// <param name="length">The length of the <see cref="MemoryStream"/> after the item has been serialized and the resulting data has been written to the stream.</param>
        protected static void Serialize<T>(T item, out MemoryStream stream, out int length)
        {
            stream = new MemoryStream();
            using (var textWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    Serializer.Serialize(jsonWriter, item);
                    jsonWriter.Flush();
                }
            }

            length = (int)stream.Position;
            stream.Position = 0;
        }

        private Task SendAsync()
        {
            // determine if we know the length we can send and whether we can tell if this is the end
            bool isLengthKnown = IsEnd;

            var header = new Header()
            {
                Type = Type,
                Id = Id,
                PayloadLength = 0,      // this value is updated by the sender when isLengthKnown is false
                End = IsEnd,             // this value is updated by the sender when isLengthKnown is false
            };

            if (StreamLength.HasValue)
            {
                // determine how many bytes we can send and if we are at the end
                header.PayloadLength = (int)Math.Min(StreamLength.Value - SendOffset, TransportConstants.MaxPayloadLength);
                header.End = SendOffset + header.PayloadLength >= StreamLength.Value;
                isLengthKnown = true;
            }

            Sender.SendPayload(header, Stream, isLengthKnown, OnSentAsync);

            return _taskCompletionSource.Task;
        }

        private async Task OnSentAsync(Header header)
        {
            SendOffset += header.PayloadLength;
            IsEnd = header.End;

            if (IsEnd)
            {
                _taskCompletionSource.SetResult(true);
            }
            else
            {
                await SendAsync().ConfigureAwait(false);
            }
        }
    }
}
