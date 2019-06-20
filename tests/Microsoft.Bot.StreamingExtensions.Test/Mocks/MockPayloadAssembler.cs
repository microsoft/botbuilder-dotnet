using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Bot.Protocol.Payloads;

namespace Microsoft.Bot.Protocol.UnitTests.Mocks
{
    public class MockPayloadAssembler : PayloadAssembler
    {
        public MockPayloadAssembler(Guid id) : base(id)
        {
        }

        public override Stream CreatePayloadStream()
        {
            return new MemoryStream();
        }

        public override void Close()
        {
            base.Close();

        }

        public override void OnReceive(Header header, Stream stream, int contentLength)
        {
            base.OnReceive(header, stream, contentLength);
        }
    }
}
