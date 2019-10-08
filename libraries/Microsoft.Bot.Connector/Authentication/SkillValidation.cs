// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector.Authentication
{
    public class SkillValidation
    {
        public static readonly TokenValidationParameters ToBotFromChannelTokenValidationParameters =
            new TokenValidationParameters()
            {
                ValidateIssuer = true,

                // TODO: not sure why I need to set this if the valid issuer is already in the array below.
                ValidIssuer = "https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/",
                ValidIssuers = new[] { AuthenticationConstants.ToBotFromChannelTokenIssuer },

                // Audience validation takes place in JwtTokenExtractor
                ValidateAudience = false,
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

            // check if we have an app id (this will be the appId for the parent bot).
            if (token.Claims.Any(app => app.Type == "appid"))
            {
                return true;
            }

            // The Token is from the Bot Framework Emulator. Success!
            return false;
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
                throw new UnauthorizedAccessException("'ver' claim is required on Emulator Tokens.");
            }

            var appId = GetAppId(versionClaim, identity);

            // TODO: check the appId against the registered skill client IDs.

            return identity;
        }

        private static string GetAppId(Claim versionClaim, ClaimsIdentity identity)
        {
            var tokenVersion = versionClaim.Value;
            string appId;

            // The Emulator, depending on Version, sends the AppId via either the
            // appid claim (Version 1) or the Authorized Party claim (Version 2).
            if (string.IsNullOrWhiteSpace(tokenVersion) || tokenVersion == "1.0")
            {
                // either no Version or a version of "1.0" means we should look for
                // the claim in the "appid" claim.
                var appIdClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.AppIdClaim);
                if (appIdClaim == null)
                {
                    // No claim around AppID. Not Authorized.
                    throw new UnauthorizedAccessException("'appid' claim is required on Emulator Token version '1.0'.");
                }

                appId = appIdClaim.Value;
            }
            else if (tokenVersion == "2.0")
            {
                // Emulator, "2.0" puts the AppId in the "azp" claim.
                var appZClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.AuthorizedParty);
                if (appZClaim == null)
                {
                    // No claim around AppID. Not Authorized.
                    throw new UnauthorizedAccessException("'azp' claim is required on Emulator Token version '2.0'.");
                }

                appId = appZClaim.Value;
            }
            else
            {
                // Unknown Version. Not Authorized.
                throw new UnauthorizedAccessException($"Unknown Emulator Token version '{tokenVersion}'.");
            }

            return appId;
        }
    }
}
