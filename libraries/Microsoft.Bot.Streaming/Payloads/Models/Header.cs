// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
using Microsoft.Bot.Streaming.Transport;

namespace Microsoft.Bot.Streaming.Payloads
{
     /// <summary>
     /// The 48-byte, fixed size, header prefaces every payload. The header must always have the
     /// same shape, regardless of if its payload is a request, response, or content.It is a
     /// period-delimited ASCII-encoded string terminated with a newline.All headers must have
     /// these segments, and all values must be zero padded to fill the correct number of bytes:
     /// |Title Size        Description
     /// |Type            1 byte ASCII-encoded char. Describes the format of the payload(request, response, stream, etc.)
     /// |Delimiter       1 byte ASCII period character.
     /// |Length          6 bytes ASCII-encoded decimal. Size in bytes of this payload in ASCII decimal, not including the header. Zero padded.
     /// |Delimiter       1 byte ASCII period character.
     /// |ID              36 bytes ASCII-encoded hex. GUID (Request ID, Stream ID, etc.).
     /// |Delimiter       1 byte ASCII period character.
     /// |End             1 byte ASCII ‘0’ or ‘1’. Signals the end of a payload or multi-part payload.
     /// |Terminator      1 byte Hardcoded to \n .
     ///  ex: A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n end example.
     /// </summary>
    public class Header
    {
        private int _internalPayloadLength;

        /// <summary>
        /// Gets or sets the 1 byte ASCII-encoded char. Describes the format of the payload(request, response, stream, etc.).
        /// </summary>
        /// <value>
        /// The 1 byte ASCII-encoded char. Describes the format of the payload(request, response, stream, etc.).
        /// </value>
        public char Type { get; set; }

        /// <summary>
        /// Gets or sets the 6 bytes ASCII-encoded decimal. Size in bytes of this payload in ASCII decimal, not including the header. Zero padded.
        /// </summary>
        /// <value>
        /// The 6 bytes ASCII-encoded decimal. Size in bytes of this payload in ASCII decimal, not including the header. Zero padded. 
        /// </value>
        public int PayloadLength
        {
            get
            {
                return _internalPayloadLength;
            }

            set
            {
                ClampLength(value, TransportConstants.MaxLength, TransportConstants.MinLength);
                _internalPayloadLength = value;
                return;
            }
        }

        /// <summary>
        /// Gets or sets the 36 bytes ASCII-encoded hex. GUID (Request ID, Stream ID, etc.).
        /// </summary>
        /// <value>
        /// The 36 bytes ASCII-encoded hex. GUID (Request ID, Stream ID, etc.).
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the 1 byte ASCII value is ‘0’ or ‘1’. Signals the end of a payload or multi-part payload.
        /// </summary>
        /// <value>
        /// The 1 byte ASCII ‘0’ or ‘1’. Signals the end of a payload or multi-part payload.
        /// </value>
        public bool End { get; set; }

        private static void ClampLength(int value, int max, int min)
        {
            if (value > max)
            {
                throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,  "Length must be less than {0}", max));
            }

            if (value < min)
            {
                throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Length must be greater than {0}", min));
            }
        }
    }
}
