// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// An assembler used to pull raw, disjointed, bytes from the incoming stream and assemble them into their original form.
    /// </summary>
    public interface IAssembler
    {
        /// <summary>
        /// Gets a value indicating whether true if the current segment is the final segment of the stream.
        /// </summary>
        /// <value>
        /// True or False.
        /// </value>
#pragma warning disable CA1716 // Identifiers should not match keywords (we can't change this without breaking binary compat)
        bool End { get; }
#pragma warning restore CA1716 // Identifiers should not match keywords

        /// <summary>
        /// Gets the ID of this assembler.
        /// </summary>
        /// <value>
        /// A GUID.
        /// </value>
        Guid Id { get; }

        /// <summary>
        /// Closes the assembler.
        /// </summary>
        void Close();

        /// <summary>
        /// Creates a new stream populated with the bytes of the assembler's payload.
        /// </summary>
        /// <returns>The new stream ready for consumption.</returns>
        Stream CreateStreamFromPayload();

        /// <summary>
        /// Returns the assembler's payload as a stream.
        /// </summary>
        /// <returns>A stream conversion of the assembler's payload.</returns>
        Stream GetPayloadAsStream();

        /// <summary>
        /// The action the assembler executes when new bytes are received on the incoming stream.
        /// </summary>
        /// <param name="header">The stream's <see cref="Header"/>.</param>
        /// <param name="stream">The incoming stream being assembled.</param>
        /// <param name="contentLength">The length of the stream, if finite.</param>
        void OnReceive(Header header, Stream stream, int contentLength);
    }
}
