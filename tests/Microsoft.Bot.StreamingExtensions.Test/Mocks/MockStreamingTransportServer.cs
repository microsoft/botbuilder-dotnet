// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Transport;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Mocks
{
    public class MockStreamingTransportServer : IStreamingTransportServer
    {
        public event DisconnectedEventHandler Disconnected;

        public Dictionary<StreamingRequest, ReceiveResponse> Messages { get; set; } = new Dictionary<StreamingRequest, ReceiveResponse>();

        public void Disconnect()
        {
            Disconnected?.Invoke(this, DisconnectedEventArgs.Empty);
        }

        public Task<ReceiveResponse> SendAsync(StreamingRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Messages[request]);
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
