// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Streaming
{
    /// <summary>
    /// Interface for classes that process streaming activities.
    /// </summary>
    public interface IStreamingActivityProcessor
    {
        /// <summary>
        /// Defines the contract for processing streaming activities.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to process.</param>
        /// <param name="botCallbackHandler">The <see cref="BotCallbackHandler"/> that will handle the activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task returning an <see cref="InvokeResponse"/> object.</returns>
        Task<InvokeResponse> ProcessStreamingActivityAsync(Activity activity, BotCallbackHandler botCallbackHandler, CancellationToken cancellationToken = default);
    }
}
