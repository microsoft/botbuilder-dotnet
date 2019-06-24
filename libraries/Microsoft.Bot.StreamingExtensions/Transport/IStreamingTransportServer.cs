// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    public interface IStreamingTransportServer
    {
        event DisconnectedEventHandler Disconnected;

        Task StartAsync();

        Task<ReceiveResponse> SendAsync(Request request, CancellationToken cancellationToken = default(CancellationToken));
    }
}
