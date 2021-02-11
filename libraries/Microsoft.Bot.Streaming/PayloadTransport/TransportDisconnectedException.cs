// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Streaming.PayloadTransport
{
    /// <summary>
    /// Exception throw when the transport is disconnected.
    /// </summary>
    public class TransportDisconnectedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransportDisconnectedException"/> class.
        /// </summary>
        public TransportDisconnectedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportDisconnectedException"/> class.
        /// </summary>
        /// <param name="message">A message describing the reason for the exception.</param>
        public TransportDisconnectedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportDisconnectedException"/> class.
        /// </summary>
        /// <param name="message">A message describing the reason for the exception.</param>
        /// <param name="innerException">A reference to an inner exception that caused this exception exception.</param>
        public TransportDisconnectedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the reason for the exception.
        /// </summary>
        /// <value>
        /// The reason for the exception.
        /// </value>
        public string Reason => Message;
    }
}
