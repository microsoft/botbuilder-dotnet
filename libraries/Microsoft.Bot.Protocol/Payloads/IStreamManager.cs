using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol.Payloads;
using Microsoft.Bot.Protocol.Utilities;

namespace Microsoft.Bot.Protocol.Payloads
{
    public interface IStreamManager
    {
        ContentStreamAssembler GetPayloadAssembler(Guid id);

        Stream GetPayloadStream(Header header);

        void OnReceive(Header header, Stream contentStream, int contentLength);

        void CloseStream(Guid id);
    }
}
