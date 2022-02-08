// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A custom exception for invoke response errors.
    /// </summary>
    public class InvokeResponseException : Exception
    {
        private readonly HttpStatusCode _statusCode;
        private readonly object _body;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeResponseException"/> class.
        /// </summary>
        /// <param name="statusCode">The Http status code of the error.</param>
        /// <param name="body">The body of the exception. Default is null.</param>
        public InvokeResponseException(HttpStatusCode statusCode, object body = null)
        {
            _statusCode = statusCode;
            _body = body;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeResponseException"/> class.
        /// </summary>
        public InvokeResponseException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeResponseException"/> class.
        /// </summary>
        /// <param name="message">The message that explains the reason for the exception, or an empty string.</param>
        public InvokeResponseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeResponseException"/> class.
        /// </summary>
        /// <param name="message">The message that explains the reason for the exception, or an empty string.</param>
        /// <param name="innerException">Gets the System.Exception instance that caused the current exception.</param>
        public InvokeResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// A factory method that creates a new <see cref="InvokeResponse"/> object with the status code and body of the current object..
        /// </summary>
        /// <returns>A new <see cref="InvokeResponse"/> object.</returns>
        public InvokeResponse CreateInvokeResponse()
        {
            return new InvokeResponse
            {
                Status = (int)_statusCode,
                Body = _body
            };
        }
    }
}
