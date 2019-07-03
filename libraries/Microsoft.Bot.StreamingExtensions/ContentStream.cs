// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Bot.StreamingExtensions.Payloads;

namespace Microsoft.Bot.StreamingExtensions
{
    internal class ContentStream : IContentStream
    {
        private readonly ContentStreamAssembler _assembler;

        internal ContentStream(Guid id, ContentStreamAssembler assembler)
        {
            Id = id;
            _assembler = assembler ?? throw new ArgumentNullException();
            Stream = _assembler.GetPayloadAsStream();
        }

        public Guid Id { get; private set; }

        public string ContentType { get; set; }

        public int? Length { get; set; }

        public Stream Stream { get; private set; }

        public void Cancel() => _assembler.Close();
    }
}
