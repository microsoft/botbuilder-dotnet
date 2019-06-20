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
    public class CancelDisassembler
    {
        private IPayloadSender Sender { get; set; }

        private Guid Id { get; set; }

        private char Type { get; set; }
        
        public CancelDisassembler(IPayloadSender sender, Guid id, char type)
        {
            Sender = sender;
            Id = id;
            Type = type;
        }
        
        public Task Disassemble()
        {            
            var header = new Header()
            {
                Type = Type,
                Id = Id,
                PayloadLength = 0,
                End = true
            };

            Sender.SendPayload(header, null, true, null);
         
            return Task.CompletedTask;
        }
    }
}
