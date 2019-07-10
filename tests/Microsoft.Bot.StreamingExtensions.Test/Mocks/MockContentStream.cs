// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Bot.StreamingExtensions.Payloads;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Mocks
{
    public class MockContentStream : IContentStream
    {
        public MockContentStream(Stream stream, string type, int? length = null)
        {
            Id = Guid.NewGuid();
            Stream = stream;
            ContentType = type;
            Length = length;
        }

        public Guid Id { get; set; }

        public string ContentType { get; set; }

        public int? Length { get; set; }

        public Stream Stream { get; set; }
    }
}
