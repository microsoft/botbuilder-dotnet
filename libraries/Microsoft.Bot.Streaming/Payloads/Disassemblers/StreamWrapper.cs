// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// A wrapper class containing a stream and the length of that stream. Used to determine the length of a stream without touching the stream itself.
    /// </summary>
    public class StreamWrapper
    {
        /// <summary>
        /// Gets or sets the stream for this wrapper.
        /// </summary>
        /// <value>
        /// The stream for this wrapper.
        /// </value>
        public Stream Stream { get; set; }

        /// <summary>
        /// Gets or sets the length of the associated stream.
        /// </summary>
        /// <value>
        /// The length of the associated stream.
        /// </value>
        public int? StreamLength { get; set; }
    }
}
