// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Validates JWT tokens sent to and from a Skill.
    /// </summary>
    public class SkillValidation
    {
        /// <summary>
        /// TO SKILL FROM BOT and TO BOT FROM SKILL: Token validation parameters when connecting a bot to a skill.
        /// </summary>
        public static readonly TokenValidationParameters ToBotFromChannelTokenValidationParameters =
            new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    "https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/", // Auth v3.1, 1.0 token
                    "https://login.microsoftonline.com/d6d49420-f39b-4df7-a1dc-d59a935871db/v2.0", // Auth v3.1, 2.0 token
                    "https://sts.windows.net/f8cdef31-a31e-4b4a-93e4-5f571e91255a/", // Auth v3.2, 1.0 token
                    "https://login.microsoftonline.com/f8cdef31-a31e-4b4a-93e4-5f571e91255a/v2.0", // Auth v3.2, 2.0 token
                    "https://sts.windows.net/cab8a31a-1906-4287-a0d8-4eef66b95f6e/", // Auth for US Gov, 1.0 token
                    "https://login.microsoftonline.us/cab8a31a-1906-4287-a0d8-4eef66b95f6e/v2.0", // Auth for US Gov, 2.0 token
                },
                ValidateAudience = false,   // Audience validation takes place manually in code.
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
            };

        public static bool IsSkillToken(string authHeader)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                // No token. Can't be an emulator token.
                return false;
            }

            var parts = authHeader.Split(' ');
            if (parts.Length != 2)
            {
                // Skill tokens MUST have exactly 2 parts. If we don't have 2 parts, it's not an emulator token
                return false;
            }

            var authScheme = parts[0];
            var bearerToken = parts[1];

            // We now have an array that should be:
            // [0] = "Bearer"
            // [1] = "[Big Long String]"
            if (authScheme != "Bearer")
            {
                // The scheme from the emulator MUST be "Bearer"
                return false;
            }

            // Parse the Big Long String into an actual token.
            var token = new JwtSecurityToken(bearerToken);

            return IsSkillClaim(token.Claims);
        }

        /// <summary>
        /// Checks if the given list of claims is a skill claim.
        /// </summary>
        /// <remarks>
        /// A skill claim should contain:
        ///     An <see cref="AuthenticationConstants.VersionClaim"/> claim.
        ///     An <see cref="AuthenticationConstants.AudienceClaim"/> claim.
        ///     An <see cref="AuthenticationConstants.AppIdClaim"/> claim (v1) or an a <see cref="AuthenticationConstants.AuthorizedParty"/> claim (v2).
        /// And the appId claim should be different than the audience claim.
        /// </remarks>
        /// <param name="claims">A list of claims.</param>
        /// <returns>True if the list of claims is a skill claim, false if is not.</returns>
        public static bool IsSkillClaim(IEnumerable<Claim> claims)
        {
            var claimsList = claims.ToList();
            var version = claimsList.FirstOrDefault(claim => claim.Type == AuthenticationConstants.VersionClaim);
            if (string.IsNullOrWhiteSpace(version?.Value))
            {
                return false;
            }

            var appId = GetAppId(claimsList);
            var audience = claimsList.FirstOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim)?.Value;
            
            if (AuthenticationConstants.ToBotFromChannelTokenIssuer.Equals(audience, StringComparison.InvariantCulture))
            {
                // The audience is https://api.botframework.com and not an appId.
                return false;
            }

            // Skill claims must contain and app ID and the AppID must be different than the audience.
            return !string.IsNullOrWhiteSpace(appId) && !string.IsNullOrWhiteSpace(audience) && appId != audience;
        }

        public static async Task<ClaimsIdentity> AuthenticateSkillToken(string authHeader, ICredentialProvider credentials, IChannelProvider channelProvider, HttpClient httpClient, string channelId, AuthenticationConfiguration authConfig, string serviceUrl)
        {
            // TODO: check if from skill. if yes do skill thing can reuse AuthenticateChannelToken except for the channel validation
            // Check the AppId and ensure that only works against my whitelist authConfig can have info on how to get the whitelist AuthenticationConfiguration
            // Do not call it whiteList

            if (authConfig == null)
            {
                throw new ArgumentNullException(nameof(authConfig));
            }

            var openIdMetadataUrl = channelProvider != null && channelProvider.IsGovernment() ? GovernmentAuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl : AuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl;

            var tokenExtractor = new JwtTokenExtractor(
                httpClient,
                ToBotFromChannelTokenValidationParameters,
                openIdMetadataUrl,
                AuthenticationConstants.AllowedSigningAlgorithms);

            var identity = await tokenExtractor.GetIdentityAsync(authHeader, channelId, authConfig.RequiredEndorsements);
            if (identity == null)
            {
                // No valid identity. Not Authorized.
                throw new UnauthorizedAccessException("Invalid Identity");
            }

            if (!identity.IsAuthenticated)
            {
                // The token is in some way invalid. Not Authorized.
                throw new UnauthorizedAccessException("Token Not Authenticated");
            }

            // Now check that the AppID in the claimset matches
            // what we're looking for. Note that in a multi-tenant bot, this value
            // comes from developer code that may be reaching out to a service, hence the
            // Async validation.
            var versionClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.VersionClaim);
            if (versionClaim == null)
            {
                throw new UnauthorizedAccessException($"'{AuthenticationConstants.VersionClaim}' claim is required on skill Tokens.");
            }

            var appId = GetAppId(identity.Claims);

            // TODO: check the appId against the registered skill client IDs.

            return identity;
        }

        /// <summary>
        /// Gets the AppId from the token claims.
        /// </summary>
        /// <remarks>
        /// In v1 tokens the AppId is in the the <see cref="AuthenticationConstants.AppIdClaim"/> claim.
        /// In v2 tokens the AppId is in the azp <see cref="AuthenticationConstants.AuthorizedParty"/> claim.
        /// If the <see cref="AuthenticationConstants.VersionClaim"/> is present, this method will attempt to
        /// obtain the attribute from the <see cref="AuthenticationConstants.AppIdClaim"/> if present.
        /// </remarks>
        /// <param name="claims">A list of claims.</param>
        /// <returns>The value of the appId claim if found (null if it can't find a suitable claim)</returns>
        public static string GetAppId(IEnumerable<Claim> claims)
        {
            var claimsList = claims.ToList();
            var tokenVersion = claimsList.FirstOrDefault(claim => claim.Type == AuthenticationConstants.VersionClaim)?.Value;
            string appId = null;

            // Depending on Version, the is either in the
            // appid claim (Version 1) or the Authorized Party claim (Version 2).
            if (string.IsNullOrWhiteSpace(tokenVersion) || tokenVersion == "1.0")
            {
                // either no Version or a version of "1.0" means we should look for
                // the claim in the "appid" claim.
                var appIdClaim = claimsList.FirstOrDefault(c => c.Type == AuthenticationConstants.AppIdClaim);
                appId = appIdClaim?.Value;
            }
            else if (tokenVersion == "2.0")
            {
                // "2.0" puts the AppId in the "azp" claim.
                var appZClaim = claimsList.FirstOrDefault(c => c.Type == AuthenticationConstants.AuthorizedParty);
                appId = appZClaim?.Value;
            }

            return appId;
        }
    }
}
