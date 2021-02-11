// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// A set of well known definitions of <see cref="PayloadStream"/> types used by <see cref="Header"/>s.
    /// </summary>
    public static class PayloadTypes
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
        /// 'S' to mark a <see cref="PayloadStream"/>.
        /// </summary>
        public const char Stream = 'S';
        
        /// <summary>
        /// 'X' to cancel all in progress communication.
        /// </summary>
        public const char CancelAll = 'X';
        
        /// <summary>
        /// 'C' to cancel a specific <see cref="PayloadStream"/>.
        /// </summary>
        public const char CancelStream = 'C';

        /// <summary>
        /// A helper method for checking if a <see cref="Header"/> represents a <see cref="PayloadStream"/>.
        /// </summary>
        /// <param name="header">The header to type check.</param>
        /// <returns>True if the header represents a <see cref="PayloadStream"/>, otherwise false.</returns>
        public static bool IsStream(Header header)
        {
            return header.Type == Stream;
        }
    }
}
