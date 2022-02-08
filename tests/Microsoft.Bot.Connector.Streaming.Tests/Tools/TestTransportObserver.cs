// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Collections.Generic;

namespace Microsoft.Bot.Connector.Streaming.Tests.Features
{
    internal class TestTransportObserver : IObserver<(Payloads.Header Header, ReadOnlySequence<byte> Payload)>
    {
        public List<(Payloads.Header Header, byte[] Payload)> Received { get; private set; } = new List<(Payloads.Header Header, byte[] Payload)>();

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext((Payloads.Header Header, ReadOnlySequence<byte> Payload) value)
        {
            Received.Add((value.Header, value.Payload.ToArray()));
        }
    }
}
