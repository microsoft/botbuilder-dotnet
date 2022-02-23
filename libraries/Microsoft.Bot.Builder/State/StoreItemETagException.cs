// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.State
{
    /// <summary>
    /// Exception thrown by an <see cref="IStorage"/> implementation when
    /// an ETag conflict exeption occurs.
    /// </summary>
    public class StoreItemETagException : InvalidOperationException
    {
        /// <summary>
        /// Exception message for <see cref="StoreItemETagException"/> when the ETag is an empty string.
        /// </summary>
        public const string ETagShouldNotBeEmptyExceptionMessage = "ETag should not be an empty string.";

        /// <summary>
        /// Exception message for <see cref="StoreItemETagException"/> when the ETag does not match the expected value.
        /// </summary>
        public const string ETagConflictExceptionMessage = "ETag conflict. ETag is expected to match the original value.";

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreItemETagException"/> class.
        /// </summary>
        public StoreItemETagException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreItemETagException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public StoreItemETagException(string message) 
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreItemETagException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">The inner exception, which caused the etag to be thrown.</param>
        public StoreItemETagException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
