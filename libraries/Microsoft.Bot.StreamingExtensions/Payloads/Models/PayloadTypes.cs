using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Protocol.Payloads
{
    public static class PayloadTypes
    {
        public const char Request       = 'A';
        public const char Response      = 'B';
        public const char Stream        = 'S';
        public const char CancelAll     = 'X';
        public const char CancelStream  = 'C';

        public static bool IsStream(Header header)
        {
            return header.Type == Stream;
        }
    }
}
