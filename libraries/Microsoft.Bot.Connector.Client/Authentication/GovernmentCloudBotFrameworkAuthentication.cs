// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using Microsoft.Bot.Connector.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Client.Authentication
{
    internal class GovernmentCloudBotFrameworkAuthentication : BuiltinBotFrameworkAuthentication
    {
        public GovernmentCloudBotFrameworkAuthentication(ServiceClientCredentialsFactory credentialFactory, AuthenticationConfiguration authConfiguration, IHttpClientFactory httpClientFactory, ILogger logger = null)
            : base(
                  GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope,
                  GovernmentAuthenticationConstants.ToChannelFromBotLoginUrl,
                  CallerIdConstants.USGovChannel,
                  GovernmentAuthenticationConstants.ChannelService,
                  GovernmentAuthenticationConstants.OAuthUrlGov,
                  credentialFactory,
                  authConfiguration,
                  httpClientFactory,
                  logger)
        {
        }
    }
}
