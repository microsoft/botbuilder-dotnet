using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.PayloadTransport;
using Microsoft.Bot.StreamingExtensions.Transport;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    public class HttpContentStreamDisassembler : PayloadDisassembler
    {
        public HttpContentStream ContentStream { get; private set; }

        public override char Type => PayloadTypes.Stream;

        public HttpContentStreamDisassembler(IPayloadSender sender, HttpContentStream contentStream)
            : base(sender, contentStream.Id)
        {
            ContentStream = contentStream;
        }

        public override async Task<StreamWrapper> GetStream()
        {
            var stream = await ContentStream.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var description = GetStreamDescription(ContentStream);

            return new StreamWrapper()
            {
                Stream = stream,
                StreamLength = description.Length
            };
        }
    }
}
