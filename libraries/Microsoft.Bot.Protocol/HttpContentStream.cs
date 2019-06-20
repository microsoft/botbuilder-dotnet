using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Bot.Protocol
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
