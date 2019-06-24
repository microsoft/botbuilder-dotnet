using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    internal interface IRequestManager
    {
        Task<bool> SignalResponse(Guid requestId, ReceiveResponse response);

        Task<ReceiveResponse> GetResponseAsync(Guid requestId, CancellationToken cancellationToken);
    }
}
