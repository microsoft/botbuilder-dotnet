// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;

namespace Microsoft.Bot.Connector.Authentication
{
    internal class PublicCloudBotFrameworkAuthentication : BuiltinBotFrameworkAuthentication
    {
        public PublicCloudBotFrameworkAuthentication(IAuthorizationHeaderProvider tokenProvider, ServiceClientCredentialsFactory credentialFactory, AuthenticationConfiguration authConfiguration, IHttpClientFactory httpClientFactory, ILogger logger)
            : base(
                  tokenProvider,
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
