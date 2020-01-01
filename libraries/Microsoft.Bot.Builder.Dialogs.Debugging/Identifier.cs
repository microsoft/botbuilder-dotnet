// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public static class Identifier
    {
        private const ulong MORE = 0x80;
        private const ulong DATA = 0x7F;

        public static ulong Encode(ulong one, ulong two)
        {
            ulong target = 0;
            int offset = 0;
            Encode(one, ref target, ref offset);
            Encode(two, ref target, ref offset);
            return target;
        }

        public static void Decode(ulong item, out ulong one, out ulong two)
        {
            Decode(ref item, out one);
            Decode(ref item, out two);
        }

        private static void Encode(ulong source, ref ulong target, ref int offset)
        {
            while (source > DATA)
            {
                ulong chunk = (byte)(source | MORE);
                target |= chunk << offset;
                offset += 8;
                source >>= 7;
            }

            {
                ulong chunk = (byte)source;
                target |= chunk << offset;
                offset += 8;
            }
        }

        private static void Decode(ref ulong source, out ulong target)
        {
            target = 0;
            int offset = 0;
            while (true)
            {
                ulong chunk = (byte)source;
                target |= (chunk & DATA) << offset;
                source >>= 8;

                if ((chunk & MORE) == 0)
                {
                    break;
                }

                offset += 7;
            }
        }
    }
}
