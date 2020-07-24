// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Custom throttling exception.
    /// </summary>
    public class ThrottleException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleException"/> class.
        /// </summary>
        public ThrottleException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleException"/> class with an exception message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public ThrottleException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleException"/> class with an exception message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ThrottleException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets or sets the property that defines the retrying behavior.
        /// </summary>
        /// <value>
        /// The property that defines the retrying behavior.
        /// </value>
        public RetryParams RetryParams { get; set; }
    }
}
