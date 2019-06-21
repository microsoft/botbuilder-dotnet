using System;
using System.IO;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Mocks
{
    public class MockContentStream : IContentStream
    {
        public MockContentStream(Stream stream, string type, int? length = null)
        {
            Id = Guid.NewGuid();
            Stream = stream;
            Type = type;
            Length = length;
        }

        public Guid Id { get; set; }

        public string Type { get; set; }

        public int? Length { get; set; }

        private Stream Stream { get; set; }

        public Stream GetStream()
        {
            return Stream;
        }
    }
}
