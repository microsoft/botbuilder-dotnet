// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    public class SimpleChannelProvider : IChannelProvider
    {
        public string ChannelService { get; set; }

        /// <summary>
        /// Creates a SimpleChannelProvider with no ChannelService which will use Public Azure
        /// </summary>
        public SimpleChannelProvider()
        {
        }

        /// <summary>
        /// Creates a SimpleChannelProvider with the specified ChannelService
        /// </summary>
        /// <param name="channelService">The ChannelService to use. Null or empty strings represent Public Azure, the string 'https://botframework.us' represents US Government Azure, and other values are for private channels.</param>
        public SimpleChannelProvider(string channelService)
        {
            this.ChannelService = channelService;
        }

        /// <summary>
        /// Gets the channel service property for this channel provider
        /// </summary>
        /// <returns>The channel service property for the channel provider</returns>
        public Task<string> GetChannelServiceAsync()
        {
            return Task.FromResult(this.ChannelService);
        }
        
        /// <summary>
        /// Gets a value of whether this provider represents a channel on US Government Azure
        /// </summary>
        /// <returns>True if this channel provider represents a channel on US Government Azure</returns>
        public bool IsGovernment()
        {
            return string.Equals(GovernmentAuthenticationConstants.ChannelService, ChannelService, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets a value of whether this provider represents a channel on Public Azure
        /// </summary>
        /// <returns>True if this channel provider represents a channel on Public Azure</returns>
        public bool IsPublicAzure()
        {
            return string.IsNullOrEmpty(ChannelService);
        }
    }
}
