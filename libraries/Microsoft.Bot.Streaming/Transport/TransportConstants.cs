// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Streaming.Transport
{
    /// <summary>
    /// A collection of constants useful when working with <see cref="Microsoft.Bot.Streaming.Payloads.Header"/>s.
    /// </summary>
    public static class TransportConstants
    {
        /// <summary>
        /// The maximum length of a single payload segment.
        /// </summary>
        public const int MaxPayloadLength = 4096;

        /// <summary>
        /// The maximum length of a <see cref="Microsoft.Bot.Streaming.Payloads.Header"/>.
        /// </summary>
        public const int MaxHeaderLength = 48;

        /// <summary>
        /// The maximum possible length of a data buffer containing a <see cref="Microsoft.Bot.Streaming.Payloads.PayloadStream"/>.
        /// </summary>
        public const int MaxLength = 999999;

        /// <summary>
        /// The minimum possible length of a data buffer containing a <see cref="Microsoft.Bot.Streaming.Payloads.PayloadStream"/>.
        /// </summary>
        public const int MinLength = 0;
    }
}
