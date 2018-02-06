// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Connector
{
    public sealed class BotAuthenticationOptions : JwtBearerOptions
    {
        /// <summary>
        /// The <see cref="ICredentialProvider"/> used for authentication.
        /// </summary>
        public ICredentialProvider CredentialProvider { set; get; }

        /// <summary>
        /// The OpenId configuation.
        /// </summary>
        public string OpenIdConfiguration { set; get; } = AuthenticationConstants.ToBotFromChannelOpenIdMetadataUrl;

        /// <summary>
        /// Flag indicating if emulator tokens should be disabled.
        /// </summary>
        public bool DisableEmulatorTokens { set; get; } = false;
    }
}