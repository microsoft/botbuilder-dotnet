// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Bot.StreamingExtensions.Transport;

#if SIGNASSEMBLY
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(
    "Microsoft.Bot.StreamingExtensions.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
#else
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(
    "Microsoft.Bot.StreamingExtensions.Tests")]
#endif

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
     /*
     * The 48-byte, fixed size, header prefaces every payload. The header must always have the
     * same shape, regardless of if its payload is a request, response, or content. It is a
     * period-delimited ASCII-encoded string terminated with a newline. All headers must have
     * these segments, and all values must be zero padded to fill the correct number of bytes:
     * |Title           Size        Description
     * |Type            1 byte      ASCII-encoded char. Describes the format of the payload (request, response, stream, etc.)
     * |Delimiter       1 byte      ASCII period character
     * |Length          6 bytes     ASCII-encoded decimal. Size in bytes of this payload in ASCII decimal, not including the header. Zero padded.
     * |Delimiter       1 byte      ASCII period character
     * |ID              36 bytes    ASCII-encoded hex. GUID (Request ID, Stream ID, etc.)
     * |Delimiter       1 byte      ASCII period character
     * |End             1 byte      ASCII ‘0’ or ‘1’. Signals the end of a payload or multi-part payload
     * |Terminator      1 byte      Hardcoded to \n
     *
     * ex: A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n
     */
    internal class Header
    {
        private int internalPayloadLength;

        public char Type { get; set; }

        public int PayloadLength
        {
            get
            {
                return internalPayloadLength;
            }

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
