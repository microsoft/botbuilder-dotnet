using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Protocol.Payloads
{
    public interface IPayloadTypeManager
    {
        PayloadAssembler CreatePayloadAssembler(Header header);
    }
}
