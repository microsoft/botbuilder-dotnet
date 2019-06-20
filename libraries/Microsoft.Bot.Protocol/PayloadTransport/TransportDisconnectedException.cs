using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Protocol.PayloadTransport
{
    public class TransportDisconnectedException : Exception
    {
        public TransportDisconnectedException() : base()
        {
        }

        public TransportDisconnectedException(string reason) : base()
        {
            Reason = reason;
        }

        public string Reason { get; set; }
    }
}
