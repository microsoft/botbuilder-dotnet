// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;

namespace Microsoft.Bot.StreamingExtensions
{
    /// <summary>
    /// An attachment contained within a <see cref="StreamingRequest"/>'s stream collection,
    /// which itself contains any form of media item.
    /// </summary>
    public class HttpContentStream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContentStream"/> class.
        /// and assigns an unique guid as its Id.
        /// </summary>
        public HttpContentStream()
            : this(Guid.NewGuid())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContentStream"/> class.
        /// </summary>
        /// <param name="id">A <see cref="Guid"/> to assign as the Id of this instance of <see cref="HttpContentStream"/>.
        /// If null a new <see cref="Guid"/> will be generated.
        /// </param>
        public HttpContentStream(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> used to identify this <see cref="HttpContentStream"/>.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> used to identify this <see cref="HttpContentStream"/>.
        /// </value>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpContent"/> of this <see cref="HttpContentStream"/>.
        /// </summary>
        /// <value>
        /// The <see cref="HttpContent"/> of this <see cref="HttpContentStream"/>.
        /// </value>
        public HttpContent Content { get; set; }
    }
}
