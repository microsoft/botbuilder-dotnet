using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    public interface ITransportReceiver : ITransport
    {
        Task<int> ReceiveAsync(byte[] buffer, int offset, int count);
    }
}
