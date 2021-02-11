// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// A stream of fixed or infinite length containing content to be decoded.
    /// </summary>
    public class ContentStream : IContentStream
    {
        private readonly PayloadStreamAssembler _assembler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentStream"/> class.
        /// </summary>
        /// <param name="id">The ID assigned to this instance.</param>
        /// <param name="assembler">The <see cref="PayloadStreamAssembler"/> assigned to this instance.</param>
        public ContentStream(Guid id, PayloadStreamAssembler assembler)
        {
            Id = id;
            _assembler = assembler ?? throw new ArgumentNullException(nameof(assembler));
            Stream = _assembler.GetPayloadAsStream();
        }

        /// <inheritdoc/>
        public Guid Id { get; private set; }

        /// <inheritdoc/>
        public string ContentType { get; set; }

        /// <inheritdoc/>
        public int? Length { get; set; }

        /// <inheritdoc/>
        public Stream Stream { get; private set; }

        /// <summary>
        /// Called to cancel processing of this instance. Calls Close() on the assigned <see cref="PayloadStreamAssembler"/>.
        /// </summary>
        public void Cancel() => _assembler.Close();
    }
}
