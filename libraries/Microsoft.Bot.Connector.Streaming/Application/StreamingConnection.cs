// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    /// <summary>
    /// A streaming based connection that can listen for incoming requests and send them to a <see cref="RequestHandler"/>, 
    /// and can also send requests to the other end of the connection.
    /// </summary>
    public abstract class StreamingConnection
    {
        /// <summary>
        /// Sends a streaming request through the connection.
        /// </summary>
        /// <param name="request"><see cref="StreamingRequest"/> to be sent.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel the send process.</param>
        /// <returns>The <see cref="ReceiveResponse"/> returned from the client.</returns>
        public abstract Task<ReceiveResponse> SendStreamingRequestAsync(StreamingRequest request, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Opens the <see cref="StreamingConnection"/> and listens for incoming requests, which will
        /// be assembled and sent to the provided <see cref="RequestHandler"/>.
        /// </summary>
        /// <param name="requestHandler"><see cref="RequestHandler"/> to which incoming requests will be sent.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that signals the need to stop the connection. 
        /// Once the token is cancelled, the connection will be gracefully shut down, finishing pending sends and receives.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task ListenAsync(RequestHandler requestHandler, CancellationToken cancellationToken = default(CancellationToken));
    }
}
