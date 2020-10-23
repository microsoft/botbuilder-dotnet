// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A base class for enqueueing an Activity for later processing.
    /// </summary>
    public abstract class QueueStorage
    {
        /// <summary>
        /// Enqueues an Activity for later processing. The visibility timeout specifies how long the message should be invisible
        /// to Dequeue and Peek operations. The message content must be a UTF-8 encoded string that is up to 64KB in size.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to be queued for later processing.</param>
        /// <param name="visibilityTimeout"> Visibility timeout.  Optional with a default value of 0.  Cannot be larger than 7 days. </param>
        /// <param name="timeToLive">Specifies the time-to-live interval for the message.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A result string.</returns>
        public abstract Task<string> QueueActivityAsync(Activity activity, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default);
    }
}
