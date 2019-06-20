using System;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    public interface ITransport : IDisposable
    {
        bool IsConnected { get; }

        void Close();
    }
}
