// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Identifiers
{
    internal static class Identifier
    {
        private const ulong More = 0x80;
        private const ulong Data = 0x7F;

        public static ulong Encode(ulong one, ulong two)
        {
            ulong target = 0;
            var offset = 0;
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
            while (source > Data)
            {
                ulong chunk = (byte)(source | More);
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
            var offset = 0;
            while (true)
            {
                ulong chunk = (byte)source;
                target |= (chunk & Data) << offset;
                source >>= 8;

                if ((chunk & More) == 0)
                {
                    break;
                }

                offset += 7;
            }
        }
    }
}
