using System;
using System.IO;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    internal interface IStreamManager
    {
        ContentStreamAssembler GetPayloadAssembler(Guid id);

        Stream GetPayloadStream(Header header);

        void OnReceive(Header header, Stream contentStream, int contentLength);

        void CloseStream(Guid id);
    }
}
