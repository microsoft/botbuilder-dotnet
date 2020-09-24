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
        /// Enqueues an Activity for later processing.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to be queued for later processing.</param>
        /// <param name="timeToLive">Specifies the time-to-live interval for the message.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A result string.</returns>
        public abstract Task<string> QueueActivityAsync(Activity activity, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default);
    }
}
