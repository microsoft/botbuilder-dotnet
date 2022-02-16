// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Connector.Streaming.Application;

namespace Microsoft.Bot.Connector.Streaming.Payloads
{
    /// <summary>
    /// A response received by <see cref="StreamingConnection"/> or <see cref="IStreamingTransportClient"/>.
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
        /// Gets the collection of <see cref="IContentStream"/>s contained within this response.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of type <see cref="IContentStream"/> containing information on streams attached to this response.
        /// </value>
        public List<IContentStream> Streams { get; private set; } = new List<IContentStream>();
    }
}
