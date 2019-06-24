// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    internal interface IStreamingTransportClient : IDisposable
    {
        event DisconnectedEventHandler Disconnected;

        bool IsConnected { get; }

        /// <summary>
        /// Establish a connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ConnectAsync();

        /// <summary>
        /// Establish a connection passing along additional headers.
        /// </summary>
        /// <param name="requestHeaders">Dictionary of header name and header value to be passed during connection. Generally, you will need channelID and Authorization.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ConnectAsync(IDictionary<string, string> requestHeaders);

        Task<ReceiveResponse> SendAsync(Request message, CancellationToken cancellationToken = default(CancellationToken));

        void Disconnect();
    }
}
