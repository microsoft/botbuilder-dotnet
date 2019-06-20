using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Protocol.Transport
{
    public interface ITransportReceiver : ITransport
    {
        Task<int> ReceiveAsync(byte[] buffer, int offset, int count);
    }
}
