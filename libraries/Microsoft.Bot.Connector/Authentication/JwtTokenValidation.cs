// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector
{
    public static class JwtTokenValidation
    {
        /// <summary>
        /// Validates the authentication header.
        /// This method ensures that AppId in claims is one supported by CredentialProvider and the service url in the claims is
        /// same as the one being passed (from activity).
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="credentials">The credentials.</param>
        /// <param name="serviceUrl">The service URL.</param>
        /// <returns>True if request is authenticated, false otherwise.</returns>
        public static async Task<bool> ValidateAuthHeader(string authHeader, ICredentialProvider credentials, string serviceUrl)
        {
            // Extract identity from token
            var identity = await GetIdentityClaim(authHeader);

            // Validate the token details.
            return await ValidateAuthHeader(identity, credentials, serviceUrl);
        }

        /// <summary>
        /// Validates the authentication header.
        /// This method ensures that AppId in claims is one supported by CredentialProvider and the service url in the claims is
        /// same as the one being passed (from activity).
        /// You can use GetIdentityClaim method to get the Claims.
        /// </summary>
        /// <param name="claimsIdentity">The claims identity.</param>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="serviceUrl">The service URL.</param>
        /// <returns>True if request is authenticated, false otherwise.</returns>
        public static async Task<bool> ValidateAuthHeader(ClaimsIdentity claimsIdentity, ICredentialProvider credentialProvider, string serviceUrl)
        {
            if (await credentialProvider.IsAuthenticationDisabledAsync())
            {
                return true;
            }

            if (claimsIdentity == null || !claimsIdentity.IsAuthenticated)
            {
                return false;
            }

            // Validate AppId (Audience claim)
            var identityAppId = claimsIdentity.GetAppIdFromClaims();
            if (!await credentialProvider.IsValidAppIdAsync(identityAppId))
            {
                return false;
            }

            // Validate serviceUrl
            // Emulator token won't have this claim, so bypass if not present
            var serviceUrlClaim = claimsIdentity.Claims.FirstOrDefault(claim => claim.Type == "serviceurl")?.Value;
            if (!string.IsNullOrWhiteSpace(serviceUrlClaim) && !string.Equals(serviceUrlClaim, serviceUrl))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the identity claim. This method just performs JWT validation. It does not validate if the token
        /// is actually meant for the Bot (AAD App) or not.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="allowEmulatorTokens">if set to <c>true</c> allow emulator tokens.</param>
        /// <returns>Claims identity if claims could be processed, or null if header could not be processed.</returns>
        public static async Task<ClaimsIdentity> GetIdentityClaim(string authHeader, bool allowEmulatorTokens = true)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                // Bot authentication is either disabled for the request is unauthenticated.
                return null;
            }

            // Extract identity from token
            var tokenExtractor = new JwtTokenExtractor(ToBotFromChannelTokenValidationParameters, JwtConfig.ToBotFromChannelOpenIdMetadataUrl, JwtConfig.ToBotFromChannelAllowedSigningAlgorithms, null);
            var identity = await tokenExtractor.GetIdentityAsync(authHeader);
            if ((identity == null || !identity.IsAuthenticated) && allowEmulatorTokens)
            {
                // No identity? If we're allowed to, fall back to MSA
                // This code path is used by the emulator
                tokenExtractor = new JwtTokenExtractor(ToBotFromEmulatorTokenValidationParameters, JwtConfig.ToBotFromEmulatorOpenIdMetadataUrl, JwtConfig.ToBotFromChannelAllowedSigningAlgorithms, null);
                identity = await tokenExtractor.GetIdentityAsync(authHeader);
            }

            return identity;
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
