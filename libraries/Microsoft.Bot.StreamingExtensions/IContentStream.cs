// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

namespace Microsoft.Bot.StreamingExtensions
{
    /// <summary>
    /// Implemented by stream attachments compatible with the Bot Framework Protocol 3 with Streaming Extensions.
    /// </summary>
    public interface IContentStream
    {
        /// <summary>
        /// Gets a guid to use as the unique identifier of this ContentStream.
        /// </summary>
        /// <value>
        /// Guid.
        /// </value>
        Guid Id { get; }

        /// <summary>
        /// Gets or sets the name of the type of the object contained within this ContentStream.
        /// </summary>
        /// <value>
        /// Plain text type name.
        /// </value>
        string Type { get; set; }

        /// <summary>
        /// Gets or sets the length of this ContentStream.
        /// </summary>
        /// <value>
        /// Null or a numeric value bound by Min/Max Int.
        /// </value>
        int? Length { get; set; }

        /// <summary>
        /// Method called to retrieve the data contained within this ContentStream.
        /// </summary>
        /// <returns>
        /// A <see cref="Stream"/> of data.
        /// </returns>
        Stream GetStream();
    }
}
