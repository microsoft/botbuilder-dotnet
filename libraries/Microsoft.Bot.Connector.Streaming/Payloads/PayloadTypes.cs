// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Streaming.Payloads
{
    internal static class PayloadTypes
    {
        /// <summary>
        /// 'A' to mark a <see cref="StreamingRequest"/>.
        /// </summary>
        public const char Request = 'A';

        /// <summary>
        /// 'B' to mark a <see cref="StreamingResponse"/>.
        /// </summary>
        public const char Response = 'B';

        /// <summary>
        /// 'S' to mark a <see cref="Stream"/>.
        /// </summary>
        public const char Stream = 'S';

        /// <summary>
        /// 'X' to cancel all in progress communication.
        /// </summary>
        public const char CancelAll = 'X';

        /// <summary>
        /// 'C' to cancel a specific <see cref="Stream"/>.
        /// </summary>
        public const char CancelStream = 'C';
    }
}
