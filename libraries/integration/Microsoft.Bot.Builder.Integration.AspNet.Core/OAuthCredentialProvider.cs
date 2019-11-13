// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public class OAuthCredentialProvider : IOAuthCredentialProvider
    {
        private const string OAuthMicrosoftAppIdKey = "oauthMicrosoftAppId";
        private const string OAuthMicrosoftPasswordKey = "oauthMicrosoftPassword";

        public OAuthCredentialProvider(IConfiguration configuration)
        {
            this.OAuthMicrosoftAppId = configuration.GetSection(OAuthMicrosoftAppIdKey)?.Value;
            this.OAuthMicrosoftAppPassword = configuration.GetSection(OAuthMicrosoftPasswordKey)?.Value;
        }

        public string OAuthMicrosoftAppId { get; set; }

        public string OAuthMicrosoftAppPassword { get; set; }
    }
}
