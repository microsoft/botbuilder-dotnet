// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector
{
    public static class JwtTokenValidation
    {
        public static async Task<bool> ValidateAuthHeader(string authHeader, string appId, string serviceUrl)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                return false;
            }

            // Extract identity from token
            var tokenExtractor = new JwtTokenExtractor(ToBotFromChannelTokenValidationParameters, JwtConfig.ToBotFromChannelOpenIdMetadataUrl, JwtConfig.ToBotFromChannelAllowedSigningAlgorithms, null);
            var identity = await tokenExtractor.GetIdentityAsync(authHeader);
            if (identity == null || !identity.IsAuthenticated)
            {
                // No identity? If we're allowed to, fall back to MSA
                // This code path is used by the emulator
                tokenExtractor = new JwtTokenExtractor(ToBotFromEmulatorTokenValidationParameters, JwtConfig.ToBotFromEmulatorOpenIdMetadataUrl, JwtConfig.ToBotFromChannelAllowedSigningAlgorithms, null);
                identity = await tokenExtractor.GetIdentityAsync(authHeader);

                if (identity == null || !identity.IsAuthenticated)
                {
                    return false;
                }
            }

            // Validate audience
            var audience = identity.Claims.FirstOrDefault(o => o.Type == "aud")?.Value;
            if (string.IsNullOrWhiteSpace(audience) || !string.Equals(audience, appId))
            {
                return false;
            }

            // Validate serviceUrl
            // Emulator token won't have this claim, so bypass if not present
            var serviceUrlClaim = identity.Claims.FirstOrDefault(claim => claim.Type == "serviceurl")?.Value;
            if (!string.IsNullOrWhiteSpace(serviceUrlClaim) && !string.Equals(serviceUrlClaim, serviceUrl))
            {
                return false;
            }

            return true;
        }

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
