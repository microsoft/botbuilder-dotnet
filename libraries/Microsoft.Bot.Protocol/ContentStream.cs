using System;
using System.IO;
using Microsoft.Bot.Protocol.Payloads;

namespace Microsoft.Bot.Protocol
{
    public class ContentStream : IContentStream
    {
        private readonly ContentStreamAssembler _assembler;
        private ConcurrentStream _stream;

        internal ContentStream(Guid id, ContentStreamAssembler assembler)
        {
            Id = id;
            _assembler = assembler ?? throw new ArgumentNullException();
        }

        public Guid Id { get; private set; }

        public string Type { get; set; }

        public int? Length { get; set; }

        public Stream GetStream()
        {
            if (_stream == null)
            {
                _stream = (ConcurrentStream)_assembler.GetPayloadStream();
            }
            return _stream;
        }

        public void Cancel()
        {
            _assembler.Close();
        }
    }
}
