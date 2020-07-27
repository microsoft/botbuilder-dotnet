// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Streaming.Transport
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
    public class TransportConstants
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        public const int MaxPayloadLength = 4096;
        public const int MaxHeaderLength = 48;
        public const int MaxLength = 999999;
        public const int MinLength = 0;
    }
}
