// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using Microsoft.Bot.Connector.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Client.Authentication
{
    internal class PublicCloudBotFrameworkAuthentication : BuiltinBotFrameworkAuthentication
    {
        public PublicCloudBotFrameworkAuthentication(ServiceClientCredentialsFactory credentialFactory, AuthenticationConfiguration authConfiguration, IHttpClientFactory httpClientFactory, ILogger logger)
            : base(
                  AuthenticationConstants.ToChannelFromBotOAuthScope,
                  AuthenticationConstants.ToChannelFromBotLoginUrlTemplate,
                  CallerIdConstants.PublicAzureChannel,
                  null,
                  AuthenticationConstants.OAuthUrl,
                  credentialFactory,
                  authConfiguration,
                  httpClientFactory,
                  logger)
        {
        }
    }
}
