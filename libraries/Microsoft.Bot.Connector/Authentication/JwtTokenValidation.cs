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
        public static async Task<bool> ValidateAuthHeader(string authHeader, string botAppId)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                return false;
            }

            // Extract identity from token
            var tokenExtractor = new JwtTokenExtractor(ToBotFromChannelTokenValidationParameters, JwtConfig.ToBotFromChannelOpenIdMetadataUrl, JwtConfig.ToBotFromChannelAllowedSigningAlgorithms, null);
            var identity = await tokenExtractor.GetIdentityAsync(authHeader);
            if (identity == null)
            {
                return false;
            }

            // Validate audience
            var audience = identity.Claims.FirstOrDefault(o => o.Type == "aud")?.Value;
            if (string.IsNullOrWhiteSpace(audience) || !string.Equals(audience, botAppId))
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
    }
}
