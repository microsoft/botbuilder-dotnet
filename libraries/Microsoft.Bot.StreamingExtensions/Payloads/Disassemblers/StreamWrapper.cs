using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Bot.Protocol.Payloads
{
    public class StreamWrapper
    {
        public Stream Stream { get; set; }

        public int? StreamLength { get; set; } 
    }
}
