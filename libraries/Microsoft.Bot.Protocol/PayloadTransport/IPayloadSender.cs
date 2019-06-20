using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol.Payloads;
using Microsoft.Bot.Protocol.Transport;

namespace Microsoft.Bot.Protocol.PayloadTransport
{
    public interface IPayloadSender
    {
        bool IsConnected { get; }

        void Connect(ITransportSender sender);

        event DisconnectedEventHandler Disconnected;

        void SendPayload(Header header, Stream payload, bool isLengthKnown, Func<Header, Task> sentCallback);

        void Disconnect(DisconnectedEventArgs e = null);
    }
}
