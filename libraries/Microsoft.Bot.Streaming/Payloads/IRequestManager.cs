// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// Manages the tasks involved in processing and responding to incoming <see cref="StreamingRequest"/>s.
    /// </summary>
    internal interface IRequestManager
    {
        /// <summary>
        /// Signal fired when all response tasks have completed.
        /// </summary>
        /// <param name="requestId">The ID of the <see cref="StreamingRequest"/>.</param>
        /// <param name="response">The <see cref="ReceiveResponse"/> in response to the request.</param>
        /// <returns>True when complete.</returns>
        Task<bool> SignalResponseAsync(Guid requestId, ReceiveResponse response);

        /// <summary>
        /// Constructs and returns a response for this request.
        /// </summary>
        /// <param name="requestId">The ID of the <see cref="StreamingRequest"/> being responded to.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The response to the specified request.</returns>
        Task<ReceiveResponse> GetResponseAsync(Guid requestId, CancellationToken cancellationToken);
    }
}
