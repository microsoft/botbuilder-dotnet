// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Transport;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Mocks
{
    public class MockTransportSender : ITransportSender
    {
        public List<ArraySegment<byte>> Buffers { get; set; } = new List<ArraySegment<byte>>();

        public bool IsConnected => true;

        public Task<int> SendAsync(byte[] buffer, int offset, int count)
        {
            Buffers.Add(new ArraySegment<byte>(buffer, offset, count));
            return Task.FromResult(count);
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }
}
