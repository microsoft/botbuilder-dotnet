using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.Bot.StreamingExtensions.Transport;

namespace Microsoft.Bot.StreamingExtensions.PayloadTransport
{
    public interface IPayloadReceiver
    {
        bool IsConnected { get; }

        event DisconnectedEventHandler Disconnected;

        void Connect(ITransportReceiver receiver);

        void Subscribe(Func<Header, Stream> getStream, Action<Header, Stream, int> receiveAction);

        void Disconnect(DisconnectedEventArgs e = null);
    }
}
