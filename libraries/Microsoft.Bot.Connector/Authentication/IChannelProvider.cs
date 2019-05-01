// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// ChannelProvider interface. This interface allows Bots to provide their own
    /// implementation for the configuration parameters to connect to a Bot.
    /// Framework channel service.
    /// </summary>
    public interface IChannelProvider
    {
        /// <summary>
        /// Gets the channel service property for this channel provider.
        /// </summary>
        /// <returns>The channel service property for the channel provider.</returns>
        Task<string> GetChannelServiceAsync();

        /// <summary>
        /// Gets a value of whether this provider represents a channel on Government Azure.
        /// </summary>
        /// <returns>True if this channel provider represents a channel on Government Azure.</returns>
        bool IsGovernment();

        /// <summary>
        /// Gets a value of whether this provider represents a channel on Public Azure.
        /// </summary>
        /// <returns>True if this channel provider represents a channel on Public Azure.</returns>
        bool IsPublicAzure();
    }
}
