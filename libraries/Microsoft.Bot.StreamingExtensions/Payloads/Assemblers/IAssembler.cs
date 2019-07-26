using System;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Microsoft.Bot.StreamingExtensions.UnitTests.Mocks")]

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    internal interface IAssembler
    {
        bool End { get; }

        Guid Id { get; }

        void Close();

        Stream CreateStreamFromPayload();

        Stream GetPayloadAsStream();

        void OnReceive(Header header, Stream stream, int contentLength);
    }
}
