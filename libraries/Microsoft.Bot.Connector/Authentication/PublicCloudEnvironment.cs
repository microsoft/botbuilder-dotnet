// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Connector.Authentication
{
    internal class PublicCloudEnvironment : BuiltinCloudEnvironment
    {
        public PublicCloudEnvironment()
            : base(AuthenticationConstants.ToChannelFromBotOAuthScope, AuthenticationConstants.ToChannelFromBotLoginUrlTemplate, CallerIdConstants.PublicAzureChannel)
        {
        }

        protected override IChannelProvider GetChannelProvider()
        {
            return null;
        }
    }
}
