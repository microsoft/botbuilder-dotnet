// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    public class SimpleChannelProvider : IChannelProvider
    {
        public string ChannelService { get; set; }

        public SimpleChannelProvider()
        {
        }

        public SimpleChannelProvider(string channelService)
        {
            this.ChannelService = channelService;
        }

        public Task<string> GetChannelServiceAsync()
        {
            return Task.FromResult(this.ChannelService);
        }

        public bool IsGovernment()
        {
            return string.Equals(AuthenticationConstants.GovernmentChannelService, ChannelService, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
