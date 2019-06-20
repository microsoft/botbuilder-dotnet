using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Protocol.Transport
{
    public class TransportConstants
    {
        public const int MaxPayloadLength = 4096;
        public const int MaxHeaderLength = 48;
        public const int MaxLength = 999999;
        public const int MinLength = 0;
    }
}
