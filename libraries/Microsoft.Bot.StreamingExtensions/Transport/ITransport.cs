using System;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    internal interface ITransport : IDisposable
    {
        bool IsConnected { get; }

        void Close();
    }
}
