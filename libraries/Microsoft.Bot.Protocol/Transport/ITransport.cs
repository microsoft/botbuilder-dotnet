using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Protocol.Transport
{
    public interface ITransport : IDisposable
    {
        bool IsConnected { get; }

        void Close();
    }
}
