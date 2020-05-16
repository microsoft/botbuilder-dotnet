using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface ITurnContextAwareStorage : IStorage
    {
        /// <summary>
        /// Call the Get API.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task<IDictionary<string, object>> ReadAsync(ITurnContext turnContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Call the Post API.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="data">The object to post.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task WriteAsync(ITurnContext turnContext, IDictionary<string, object> data, CancellationToken cancellationToken = default);
    }
}
