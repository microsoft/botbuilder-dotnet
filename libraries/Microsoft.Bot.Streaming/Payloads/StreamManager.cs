// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.IO;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// StreamManagers are used to provide access to the objects involved in processing incoming <see cref="PayloadStream"/>s.
    /// </summary>
    public class StreamManager : IStreamManager
    {
        private readonly ConcurrentDictionary<Guid, PayloadStreamAssembler> _activeAssemblers;
        private readonly Action<PayloadStreamAssembler> _onCancelStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamManager"/> class.
        /// </summary>
        /// <param name="onCancelStream">Optional action to trigger if the managed stream is cancelled.</param>
        public StreamManager(Action<PayloadStreamAssembler> onCancelStream = null)
        {
            // If no callback is defined, make it a noop to avoid null checking everywhere.
            _onCancelStream = onCancelStream ?? ((a) => { });
            _activeAssemblers = new ConcurrentDictionary<Guid, PayloadStreamAssembler>();
        }

        /// <inheritdoc/>
        public PayloadStreamAssembler GetPayloadAssembler(Guid id)
        {
            if (!_activeAssemblers.TryGetValue(id, out var assembler))
            {
                // a new id has come in, start a new task to process it
                assembler = new PayloadStreamAssembler(this, id);
                if (!_activeAssemblers.TryAdd(id, assembler))
                {
                    // Don't need to dispose the assembler because it was never used
                    // Get the one that is in use
                    _activeAssemblers.TryGetValue(id, out assembler);
                }
            }

            return assembler;
        }

        /// <inheritdoc/>
        public Stream GetPayloadStream(Header header)
        {
            var assembler = GetPayloadAssembler(header.Id);

            return assembler.GetPayloadAsStream();
        }

        /// <inheritdoc/>
        public void OnReceive(Header header, Stream contentStream, int contentLength)
        {
            if (_activeAssemblers.TryGetValue(header.Id, out var assembler))
            {
                assembler.OnReceive(header, contentStream, contentLength);
            }
        }

        /// <inheritdoc/>
        public void CloseStream(Guid id)
        {
            if (_activeAssemblers.TryRemove(id, out var assembler))
            {
                // decide whether to cancel it or not
                var stream = assembler.GetPayloadAsStream();
                if ((assembler.ContentLength.HasValue && stream.Length < assembler.ContentLength.Value) ||
                    !assembler.End)
                {
                    _onCancelStream(assembler);
                }
            }
        }
    }
}
