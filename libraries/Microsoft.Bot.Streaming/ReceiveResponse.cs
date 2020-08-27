// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.Transport;

namespace Microsoft.Bot.Streaming
{
    /// <summary>
    /// A response received by a <see cref="IStreamingTransportServer"/> or <see cref="IStreamingTransportClient"/>.
    /// </summary>
    public class ReceiveResponse
    {
        /// <summary>
        /// Gets or sets the status code of this response.
        /// </summary>
        /// <value>
        /// The numeric portion of a status code.
        /// </value>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="IContentStream"/>s contained within this response.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of type <see cref="IContentStream"/> containing information on streams attached to this response.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<IContentStream> Streams { get; set; } = new List<IContentStream>();
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
