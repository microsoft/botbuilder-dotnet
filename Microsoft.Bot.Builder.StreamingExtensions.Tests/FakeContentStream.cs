using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Bot.StreamingExtensions.Payloads;

namespace Microsoft.Bot.Builder.StreamingExtensions.Tests
{
    public class FakeContentStream : IContentStream
    {
        public FakeContentStream(Guid id, string contentType, Stream stream)
        {
            Id = id;
            ContentType = contentType;
            Stream = stream;
            Length = int.Parse(stream.Length.ToString());
        }

        public Guid Id { get; set; }

        public string ContentType { get; set; }

        public int? Length { get; set; }

        public Stream Stream { get; set; }
    }
}
