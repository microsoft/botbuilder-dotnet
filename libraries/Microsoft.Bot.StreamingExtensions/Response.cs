// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.StreamingExtensions
{
    /// <summary>
    /// Implementation split between Response and ResponseEx.
    /// The basic response type sent over Bot Framework Protocol 3 with Streaming Extensions transports,
    /// equivalent to HTTP response messages.
    /// </summary>
    public partial class Response
    {
        /// <summary>
        /// Gets or sets the numeric status code for the response,
        /// adhering to <see cref="https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode?view=netframework-4.8"/>.
        /// </summary>
        /// <value>
        /// The numeric status code for the response.
        /// </value>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the collection of streams attached to this response.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of type <see cref="HttpContentStream"/>.
        /// </value>
        public List<HttpContentStream> Streams { get; set; }
    }
}
