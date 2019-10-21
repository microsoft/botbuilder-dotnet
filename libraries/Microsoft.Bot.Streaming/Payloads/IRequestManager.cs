// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Payloads
{
    internal interface IRequestManager
    {
        Task<bool> SignalResponseAsync(Guid requestId, ReceiveResponse response);

        Task<ReceiveResponse> GetResponseAsync(Guid requestId, CancellationToken cancellationToken);
    }
}
