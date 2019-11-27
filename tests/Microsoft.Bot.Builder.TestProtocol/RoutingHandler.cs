// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.TestProtocol
{
    public class RoutingHandler : ChannelServiceHandler
    {
        public RoutingHandler(
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfiguration,
            IChannelProvider channelProvider = null)
            : base(credentialProvider, authConfiguration, channelProvider)
        {
        }

        // add overrides to call corresponding methods on the Connector
    }
}
