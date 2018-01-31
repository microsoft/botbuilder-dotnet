// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Configuration for JWT tokens
    /// </summary>
    public static class JwtConfig
    {
        /// <summary>
        /// TO CHANNEL FROM BOT: Login URL
        /// </summary>
        public const string ToChannelFromBotLoginUrl = "https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token";

        /// <summary>
        /// TO CHANNEL FROM BOT: OAuth scope to request
        /// </summary>
        public const string ToChannelFromBotOAuthScope = "https://api.botframework.com/.default";

        /// <summary>
        /// TO BOT FROM CHANNEL: OpenID metadata document for tokens coming from MSA
        /// </summary>
        public const string ToBotFromChannelOpenIdMetadataUrl = "https://login.botframework.com/v1/.well-known/openidconfiguration";

        /// <summary>
        /// TO BOT FROM CHANNEL: Token validation parameters when connecting to a bot
        /// </summary>
        public static readonly TokenValidationParameters ToBotFromChannelTokenValidationParameters =
            new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { "https://api.botframework.com" },
                // Audience validation takes place in JwtTokenExtractor
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true
            };

        /// <summary>
        /// TO BOT FROM CHANNEL: Allowed token signing algorithms
        /// </summary>
        public static readonly string[] ToBotFromChannelAllowedSigningAlgorithms = new[] { "RS256", "RS384", "RS512" };

        /// <summary>
        /// TO BOT FROM EMULATOR: OpenID metadata document for tokens coming from MSA
        /// </summary>
        public const string ToBotFromEmulatorOpenIdMetadataUrl = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

        /// <summary>
        /// TO BOT FROM EMULATOR: Token validation parameters when connecting to a channel
        /// </summary>
        public static readonly TokenValidationParameters ToBotFromEmulatorTokenValidationParameters =
            new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuers = new[] {
                    "https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/",                    // Auth v3.1, 1.0 token
                    "https://login.microsoftonline.com/d6d49420-f39b-4df7-a1dc-d59a935871db/v2.0",      // Auth v3.1, 2.0 token
                    "https://sts.windows.net/f8cdef31-a31e-4b4a-93e4-5f571e91255a/",                    // Auth v3.2, 1.0 token
                    "https://login.microsoftonline.com/f8cdef31-a31e-4b4a-93e4-5f571e91255a/v2.0"       // Auth v3.2, 2.0 token
                },
                // Audience validation takes place in JwtTokenExtractor
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true
            };
    }
}
