// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.StreamingExtensions
{
    /// <summary>
    /// Implementation split between Request and RequestEx.
    /// The basic request type sent over Bot Framework Protocol 3 with Streaming Extensions transports,
    /// equivalent to HTTP request messages.
    /// </summary>
    public partial class Request
    {
        /// <summary>
        /// Gets or sets the verb action this request will perform.
        /// </summary>
        /// <value>
        /// The string representation of an HTTP verb.
        /// </value>
        public string Verb { get; set; }

        /// <summary>
        /// Gets or sets the path this request will route to on the remote server.
        /// </summary>
        /// <value>
        /// The string representation of the URL style path to request at the remote server.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the collection of stream attachments included in this request.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of <see cref="HttpContentStream"/> items associated with this request.
        /// </value>
        public List<HttpContentStream> Streams { get; set; }
    }
}
