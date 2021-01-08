// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Microsoft.Rest;

    /// <summary>
    /// Exception thrown for an invalid response with ErrorResponse
    /// information.
    /// </summary>
    public partial class ErrorResponseException : RestException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResponseException"/> class.
        /// </summary>
        public ErrorResponseException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResponseException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public ErrorResponseException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResponseException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public ErrorResponseException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets or sets the information about the associated HTTP request.
        /// </summary>
        /// <value>Information about the associated HTTP requests.</value>
        public HttpRequestMessageWrapper Request { get; set; }

        /// <summary>
        /// Gets or sets the information about the associated HTTP response.
        /// </summary>
        /// <value>Information about the associated HTTP response.</value>
        public HttpResponseMessageWrapper Response { get; set; }

        /// <summary>
        /// Gets or sets the body object.
        /// </summary>
        /// <value>The body.</value>
        public ErrorResponse Body { get; set; }
    }
}
