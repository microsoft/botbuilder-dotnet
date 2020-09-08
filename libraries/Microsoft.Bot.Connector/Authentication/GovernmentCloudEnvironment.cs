// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    internal class GovernmentCloudEnvironment : BuiltinCloudEnvironment
    {
        public GovernmentCloudEnvironment()
            : base(GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope, GovernmentAuthenticationConstants.ToChannelFromBotLoginUrl, CallerIdConstants.USGovChannel)
        {
        }

        protected override IChannelProvider GetChannelProvider()
        {
            return new SimpleChannelProvider(GovernmentAuthenticationConstants.ChannelService);
        }
    }
}
