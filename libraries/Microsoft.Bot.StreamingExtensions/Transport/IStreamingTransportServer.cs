using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Protocol.Transport
{
    public interface IStreamingTransportServer
    {
        event DisconnectedEventHandler Disconnected;

        Task StartAsync();

        Task<ReceiveResponse> SendAsync(Request request, CancellationToken cancellationToken = default(CancellationToken));

        void Disconnect();
    }
}