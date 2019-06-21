using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Transport;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Mocks
{
    public class MockStreamingTransportServer : IStreamingTransportServer
    {
        public event DisconnectedEventHandler Disconnected;

        public Dictionary<Request, ReceiveResponse> Messages { get; set; } = new Dictionary<Request, ReceiveResponse>();

        public void Disconnect()
        {
            Disconnected?.Invoke(this, DisconnectedEventArgs.Empty);
        }

        public Task<ReceiveResponse> SendAsync(Request request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Messages[request]);
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
