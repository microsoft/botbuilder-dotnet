// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Collections.Generic;
using Microsoft.Bot.Streaming.Payloads;

namespace Microsoft.Bot.Connector.Streaming.Tests.Features
{
    internal class TestTransportObserver : IObserver<(Header Header, ReadOnlySequence<byte> Payload)>
    {
        public List<(Header Header, byte[] Payload)> Received { get; private set; } = new List<(Header Header, byte[] Payload)>();

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext((Header Header, ReadOnlySequence<byte> Payload) value)
        {
            Received.Add((value.Header, value.Payload.ToArray()));
        }
    }
}
