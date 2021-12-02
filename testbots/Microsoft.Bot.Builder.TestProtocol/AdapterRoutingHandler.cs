// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest;

namespace Microsoft.Bot.Builder.TestProtocol
{
    public class AdapterRoutingHandler : ChannelServiceHandler
    {
        private readonly SkillConversationIdFactoryBase _factory;
        private readonly ServiceClientCredentials _credentials;

        public AdapterRoutingHandler(
            SkillConversationIdFactoryBase factory,
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfiguration,
            IChannelProvider channelProvider = null)
            : base(credentialProvider, authConfiguration, channelProvider)
        {
            _factory = factory;
            _credentials = MicrosoftAppCredentials.Empty;
        }
    }
}
