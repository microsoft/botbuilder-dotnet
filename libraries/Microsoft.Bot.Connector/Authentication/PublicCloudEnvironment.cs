// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    internal class PublicCloudEnvironment : BuiltinCloudEnvironment
    {
        public PublicCloudEnvironment()
            : base(AuthenticationConstants.ToChannelFromBotOAuthScope)
        {
        }

        protected override ServiceClientCredentials CreateServiceClientCredentials(string appId, string appPassword, HttpClient httpClient, ILogger logger, string scope)
        {
            return new MicrosoftAppCredentials(appId, appPassword, httpClient, logger, scope);
        }
    }
}
