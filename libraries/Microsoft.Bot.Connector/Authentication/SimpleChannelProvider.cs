// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// A simple channel provider with basic configuration parameters to connect to a Bot Framework channel service.
    /// </summary>
    public class SimpleChannelProvider : IChannelProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleChannelProvider"/> class.
        /// Creates a SimpleChannelProvider with no ChannelService which will use Public Azure.
        /// </summary>
        public SimpleChannelProvider()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleChannelProvider"/> class.
        /// </summary>
        /// <param name="channelService">The ChannelService to use. Null or empty strings represent Public Azure, the string 'https://botframework.us' represents US Government Azure, and other values are for private channels.</param>
        public SimpleChannelProvider(string channelService)
        {
            this.ChannelService = channelService;
        }

        /// <summary>
        /// Gets or sets the channel service.
        /// </summary>
        /// <value>
        /// The channel service.
        /// </value>
        public string ChannelService { get; set; }

        /// <summary>
        /// Gets the channel service property for this channel provider.
        /// </summary>
        /// <returns>The channel service property for the channel provider.</returns>
        public Task<string> GetChannelServiceAsync()
        {
            return Task.FromResult(this.ChannelService);
        }

        /// <summary>
        /// Gets a value of whether this provider represents a channel on US Government Azure.
        /// </summary>
        /// <returns>True if this channel provider represents a channel on US Government Azure.</returns>
        public bool IsGovernment()
        {
            return string.Equals(GovernmentAuthenticationConstants.ChannelService, ChannelService, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets a value of whether this provider represents a channel on Public Azure.
        /// </summary>
        /// <returns>True if this channel provider represents a channel on Public Azure.</returns>
        public bool IsPublicAzure()
        {
            return string.IsNullOrEmpty(ChannelService);
        }
    }
}
