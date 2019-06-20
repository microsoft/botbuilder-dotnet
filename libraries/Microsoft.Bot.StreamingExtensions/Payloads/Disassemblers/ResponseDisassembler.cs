using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol.PayloadTransport;
using Microsoft.Bot.Protocol.Transport;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Protocol.Payloads
{
    public class ResponseDisassembler : PayloadDisassembler
    {
        public Response Response { get; private set; }
        
        public override char Type => PayloadTypes.Response;

        public ResponseDisassembler(IPayloadSender sender, Guid id, Response response)
            : base(sender, id)
        {
            Response = response;
        }

        public override Task<StreamWrapper> GetStream()
        {
            var payload = new ResponsePayload()
            {
                StatusCode = Response.StatusCode
            };

            if(Response.Streams != null)
            {
                payload.Streams = new List<StreamDescription>();
                foreach (var contentStream in Response.Streams)
                {
                    var description = GetStreamDescription(contentStream);

                    payload.Streams.Add(description);
                }
            }

            Serialize(payload, out MemoryStream memoryStream, out int streamLength);

            return Task.FromResult(new StreamWrapper() {
                Stream = memoryStream,
                StreamLength = streamLength
            });
        }
    }
}
