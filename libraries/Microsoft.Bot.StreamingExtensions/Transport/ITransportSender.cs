using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    public interface ITransportSender : ITransport
    {
        Task<int> SendAsync(byte[] buffer, int offset, int count);
    }
}
