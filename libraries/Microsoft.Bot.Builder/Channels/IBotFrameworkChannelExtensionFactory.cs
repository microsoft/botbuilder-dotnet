// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Channels
{
    public interface IBotFrameworkChannelExtensionFactory
    {
        /// <summary>
        /// Gets the channel identifier for the channel this extension supports.
        /// </summary>
        /// <value>
        /// The channel identifier.
        /// </value>
        string ChannelId { get; }

        /// <summary>
        /// Gets the channel extension asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Channel extension instance.</returns>
        Task<IBotFrameworkChannelExtension> GetChannelExtensionAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken);
    }
}
