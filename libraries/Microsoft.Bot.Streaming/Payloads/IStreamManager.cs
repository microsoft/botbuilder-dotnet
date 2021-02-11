// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// StreamManagers are used to provide access to the objects involved in processing incoming <see cref="PayloadStream"/>s.
    /// </summary>
    public interface IStreamManager
    {
        /// <summary>
        /// Retrieves a <see cref="PayloadStreamAssembler"/> with the given ID if one exists, otherwise a new instance is created and assigned the given ID.
        /// </summary>
        /// <param name="id">The ID of the <see cref="PayloadStreamAssembler"/> to retrieve or create.</param>
        /// <returns>The <see cref="PayloadStreamAssembler"/> with the given ID.</returns>
        PayloadStreamAssembler GetPayloadAssembler(Guid id);

        /// <summary>
        /// Retrieves the <see cref="PayloadStream"/> from the <see cref="PayloadStreamAssembler"/> this manager manages.
        /// </summary>
        /// <param name="header">The <see cref="Header"/> of the <see cref="PayloadStream"/> to retrieve.</param>
        /// <returns>The <see cref="PayloadStream"/> with the given header.</returns>
        Stream GetPayloadStream(Header header);

        /// <summary>
        /// Used to set the behavior of the managed <see cref="PayloadStreamAssembler"/> when data is received.
        /// </summary>
        /// <param name="header">The <see cref="Header"/> of the stream.</param>
        /// <param name="contentStream">The <see cref="Stream"/> to write incoming data to.</param>
        /// <param name="contentLength">The amount of data to write to the contentStream.</param>
        void OnReceive(Header header, Stream contentStream, int contentLength);

        /// <summary>
        /// Closes the <see cref="PayloadStreamAssembler"/> assigned to the <see cref="PayloadStream"/> with the given ID.
        /// </summary>
        /// <param name="id">The ID of the <see cref="PayloadStream"/> to close.</param>
        void CloseStream(Guid id);
    }
}
