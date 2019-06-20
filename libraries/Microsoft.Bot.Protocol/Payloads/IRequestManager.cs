using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Protocol.Payloads
{
    public interface IRequestManager
    {
        Task<bool> SignalResponse(Guid requestId, ReceiveResponse response);

        Task<ReceiveResponse> GetResponseAsync(Guid requestId, CancellationToken cancellationToken);
    }
}
