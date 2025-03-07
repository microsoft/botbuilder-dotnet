// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// An exception thrown when a 412 Precondition Failed error happens.
    /// </summary>
    public class ETagException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ETagException"/> class.
        /// </summary>
        public ETagException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ETagException"/> class with an exception message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public ETagException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ETagException"/> class with an exception message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ETagException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ETagException"/> class with an item key, ETag, storage ETag, and inner exception.
        /// </summary>
        /// <param name="itemKey">The key of the storage item.</param>
        /// <param name="itemETag">The ETag to set to the storage item.</param>
        /// <param name="itemStorageETag">The ETag that's currently in the storage item.</param>
        /// <param name="innerException">The inner exception.</param>
        public ETagException(string itemKey, string itemETag, string itemStorageETag, Exception innerException = null)
            : base(CreateMessage(itemKey, itemETag, itemStorageETag), innerException)
        {
        }

        /// <inheritdoc/>
        public override string Message
        {
            // Add a prefix to the message to avoid breaking existing code that looks for the message starting with "Etag conflict:"
            get
            {
                var conflictMessage = "Etag conflict:";
                return base.Message.StartsWith(conflictMessage) ? base.Message : $"{conflictMessage} {base.Message}";
            }
        }

        private static string CreateMessage(string key, string etag, string storageETag)
        {
            return $"Unable to write the Item to the Storage due to a 412 Precondition Failed error.\nThis could happen when the Item was already processed by another machine or thread to avoid conflicts or data loss.\n\nKey: {key}\nETag to write: {etag}\nETag in storage: {storageETag}";
        }
    }
}
