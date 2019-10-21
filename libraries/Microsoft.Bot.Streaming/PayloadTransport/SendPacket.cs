// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;

namespace Microsoft.Bot.Streaming.PayloadTransport
{
    internal class SendPacket
    {
        public Header Header { get; set; }

        public Stream Payload { get; set; }

        public bool IsLengthKnown { get; set; }

        public Func<Header, Task> SentCallback { get; set; }
    }
}
