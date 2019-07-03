// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Newtonsoft.Json;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    internal abstract class PayloadAssembler
    {
        private object _syncLock = new object();

        public PayloadAssembler(Guid id)
        {
            Id = id;
            End = false;
        }

        public Guid Id { get; private set; }

        public bool End { get; private set; }

        protected static JsonSerializer Serializer { get; set; } = JsonSerializer.Create(SerializationSettings.DefaultSerializationSettings);

        private Stream Stream { get; set; }

        public Stream GetPayloadAsStream()
        {
            lock (_syncLock)
            {
                if (Stream == null)
                {
                    Stream = CreateStreamFromPayload();
                }
            }

            return Stream;
        }

        public abstract Stream CreateStreamFromPayload();

        public virtual void OnReceive(Header header, Stream stream, int contentLength) => End = header.End;

        public virtual void Close()
        {
        }
    }
}
