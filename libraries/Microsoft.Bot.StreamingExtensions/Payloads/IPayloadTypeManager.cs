using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    public interface IPayloadTypeManager
    {
        PayloadAssembler CreatePayloadAssembler(Header header);
    }
}
