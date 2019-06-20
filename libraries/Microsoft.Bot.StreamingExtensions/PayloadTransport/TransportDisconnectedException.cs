using System;

namespace Microsoft.Bot.StreamingExtensions.PayloadTransport
{
    public class TransportDisconnectedException : Exception
    {
        public TransportDisconnectedException()
            : base()
        {
        }

        public TransportDisconnectedException(string reason)
            : base()
        {
            Reason = reason;
        }

        public string Reason { get; set; }
    }
}
