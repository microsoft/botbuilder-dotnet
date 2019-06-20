using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol.Payloads;
using Microsoft.Bot.Protocol.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Bot.Protocol.Payloads
{    
    public abstract class PayloadAssembler
    {
        protected static JsonSerializer Serializer = JsonSerializer.Create(SerializationSettings.DefaultSerializationSettings);

        public Guid Id { get; private set; }

        public bool End { get; private set; }

        private Stream Stream { get; set; }

        private object _syncLock = new object();

        public PayloadAssembler(Guid id)
        {
            Id = id;
            End = false;
        }
        
        public Stream GetPayloadStream()
        {
            lock (_syncLock)
            {
                if (Stream == null)
                {
                    Stream = CreatePayloadStream();
                }
            }
            return Stream;
        }

        public abstract Stream CreatePayloadStream();

        public virtual void OnReceive(Header header, Stream stream, int contentLength)
        {
            End = header.End;
        }

        public virtual void Close()
        {
        }
    }
}
