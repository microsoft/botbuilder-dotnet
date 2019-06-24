using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    internal interface ITransportReceiver : ITransport
    {
        Task<int> ReceiveAsync(byte[] buffer, int offset, int count);
    }
}
