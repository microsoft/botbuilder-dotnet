using Microsoft.Bot.StreamingExtensions.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    public class Header
    {
        private int internalPayloadLength;

        public char Type { get; set; }

        public int PayloadLength
        {
            get { return internalPayloadLength; }
            set
            {
                ClampLength(value, TransportConstants.MaxLength, TransportConstants.MinLength);
                internalPayloadLength = value;
                return;
            }
        }

        public Guid Id { get; set; }

        public bool End { get; set; }

        private void ClampLength(int value, int max, int min)
        {
            if (value > max)
            {
                throw new InvalidDataException(string.Format("Length must be less than {0}", max));
            }
            if (value < min)
            {
                throw new InvalidDataException(string.Format("Length must be greater than {0}", min));
            }

        }
    }
}
