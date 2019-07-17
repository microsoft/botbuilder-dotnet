// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Newtonsoft.Json;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Mocks
{
    internal class MockPayloadAssembler : IAssembler
    {
        public MockPayloadAssembler(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }

        public bool End { get; private set; }

        protected static JsonSerializer Serializer { get; set; } = JsonSerializer.Create(SerializationSettings.DefaultSerializationSettings);

        public Stream CreateStreamFromPayload() => new MemoryStream();

        public void Close()
        {
        }

#pragma warning disable IDE0060
        public void OnReceive(Header header, Stream stream, int contentLength) => End = header.End;
#pragma warning restore IDE0060

        public Stream GetPayloadAsStream() => throw new NotImplementedException();
    }
}
