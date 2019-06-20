using System;
using System.Net.Http;

namespace Microsoft.Bot.StreamingExtensions
{
    public class HttpContentStream
    {
        public HttpContentStream()
        {
            Id = Guid.NewGuid();
        }

        public HttpContentStream(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }

        public HttpContent Content { get; set; }
    }
}
