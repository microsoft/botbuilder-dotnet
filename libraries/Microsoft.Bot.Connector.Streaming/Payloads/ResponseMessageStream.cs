// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;

namespace Microsoft.Bot.Connector.Streaming.Payloads
{
    /// <summary>
    /// An attachment contained within a <see cref="StreamingRequest"/>'s stream collection,
    /// which itself contains any form of media item.
    /// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class ResponseMessageStream
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseMessageStream"/> class.
        /// and assigns an unique guid as its Id.
        /// </summary>
        public ResponseMessageStream()
            : this(Guid.NewGuid())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseMessageStream"/> class.
        /// </summary>
        /// <param name="id">A <see cref="Guid"/> to assign as the Id of this instance of <see cref="ResponseMessageStream"/>.
        /// If null a new <see cref="Guid"/> will be generated.
        /// </param>
        public ResponseMessageStream(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> used to identify this <see cref="ResponseMessageStream"/>.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> used to identify this <see cref="ResponseMessageStream"/>.
        /// </value>
        public Guid Id { get; }

        /// <summary>
        /// Gets or sets the <see cref="HttpContent"/> of this <see cref="ResponseMessageStream"/>.
        /// </summary>
        /// <value>
        /// The <see cref="HttpContent"/> of this <see cref="ResponseMessageStream"/>.
        /// </value>
        public HttpContent Content { get; set; }
    }
}
