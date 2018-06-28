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
    /// <summary>
    /// Validates and Examines JWT tokens from the Bot Framework Emulator
    /// </summary>
    public static class EmulatorValidation
    {
        /// <summary>
        /// TO BOT FROM EMULATOR: Token validation parameters when connecting to a channel.
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
                ValidateAudience = false,   // Audience validation takes place manually in code. 
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true
            };

        /// <summary>
        /// Determines if a given Auth header is from the Bot Framework Emulator
        /// </summary>
        /// <param name="authHeader">Bearer Token, in the "Bearer [Long String]" Format.</param>
        /// <returns>True, if the token was issued by the Emulator. Otherwise, false.</returns>
        public static bool IsTokenFromEmulator(string authHeader)
        {
            // The Auth Header generally looks like this:
            // "Bearer eyJ0e[...Big Long String...]XAiO"
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                // No token. Can't be an emulator token. 
                return false;
            }

            string[] parts = authHeader?.Split(' ');
            if (parts.Length != 2)
            {
                // Emulator tokens MUST have exactly 2 parts. If we don't have 2 parts, it's not an emulator token
                return false;
            }

            string authScheme = parts[0];
            string bearerToken = parts[1];

            // We now have an array that should be:
            // [0] = "Bearer"
            // [1] = "[Big Long String]"
            if (authScheme != "Bearer")
            {
                // The scheme from the emulator MUST be "Bearer"
                return false;
            }

            // Parse the Big Long String into an actual token. 
            JwtSecurityToken token = new JwtSecurityToken(bearerToken);

            // Is there an Issuer? 
            if (string.IsNullOrWhiteSpace(token.Issuer))
            {
                // No Issuer, means it's not from the Emulator. 
                return false;
            }

            // Is the token issues by a source we consider to be the emulator? 
            if (!ToBotFromEmulatorTokenValidationParameters.ValidIssuers.Contains(token.Issuer))
            {
                // Not a Valid Issuer. This is NOT a Bot Framework Emulator Token.                 
                return false;
            }

            // The Token is from the Bot Framework Emulator. Success! 
            return true;
        }

        /// <summary>
        /// Validate the incoming Auth Header as a token sent from the Bot Framework Emulator.         
        /// </summary>
        /// <param name="authHeader">The raw HTTP header in the format: "Bearer [longString]"</param>
        /// <param name="credentials">The user defined set of valid credentials, such as the AppId.</param>
        /// <param name="httpClient">Authentication of tokens requires calling out to validate Endorsements and related documents. The
        /// HttpClient is used for making those calls. Those calls generally require TLS connections, which are expensive to 
        /// setup and teardown, so a shared HttpClient is recommended.</param>
        /// <param name="channelId">The ID of the channel to validate.</param>
        /// <returns>
        /// A valid ClaimsIdentity. 
        /// </returns>
        /// <remarks>
        /// A token issued by the Bot Framework will FAIL this check. Only Emulator tokens will pass.
        /// </remarks>
        public static async Task<ClaimsIdentity> AuthenticateEmulatorToken(string authHeader, ICredentialProvider credentials, HttpClient httpClient, string channelId)
        {
            var tokenExtractor = new JwtTokenExtractor(
                    httpClient,
                    ToBotFromEmulatorTokenValidationParameters,
                    AuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl,
                    AuthenticationConstants.AllowedSigningAlgorithms);

            var identity = await tokenExtractor.GetIdentityAsync(authHeader, channelId);
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
            Claim versionClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.VersionClaim);
            if (versionClaim == null)
            {
                throw new UnauthorizedAccessException("'ver' claim is required on Emulator Tokens.");
            }

            string tokenVersion = versionClaim.Value;                         
            string appID = string.Empty;

            // The Emulator, depending on Version, sends the AppId via either the 
            // appid claim (Version 1) or the Authorized Party claim (Version 2). 
            if (string.IsNullOrWhiteSpace(tokenVersion) || tokenVersion == "1.0")
            {
                // either no Version or a version of "1.0" means we should look for 
                // the claim in the "appid" claim. 
                Claim appIdClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.AppIdClaim);
                if (appIdClaim == null)
                {
                    // No claim around AppID. Not Authorized.
                    throw new UnauthorizedAccessException("'appid' claim is required on Emulator Token version '1.0'.");
                }

                appID = appIdClaim.Value;
            }
            else if (tokenVersion == "2.0")
            {
                // Emulator, "2.0" puts the AppId in the "azp" claim. 
                Claim appZClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.AuthorizedParty);
                if (appZClaim == null)
                {
                    // No claim around AppID. Not Authorized.
                    throw new UnauthorizedAccessException("'azp' claim is required on Emulator Token version '2.0'.");
                }

                appID = appZClaim.Value;
            }
            else
            {
                // Unknown Version. Not Authorized. 
                throw new UnauthorizedAccessException($"Unknown Emulator Token version '{tokenVersion}'.");
            }

            if (!await credentials.IsValidAppIdAsync(appID))
            {
                throw new UnauthorizedAccessException($"Invalid AppId passed on token: {appID}");
            }

            return identity;
        }
    }
}
