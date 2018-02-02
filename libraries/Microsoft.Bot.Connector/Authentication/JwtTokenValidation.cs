// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector.Authentication
{
    public static class JwtTokenValidation
    {
        public static async Task<ClaimsIdentity> ValidateAuthHeader(string authHeader, ICredentialProvider credentials, string serviceUrl)
        {            
            if (string.IsNullOrWhiteSpace(authHeader) && await credentials.IsAuthenticationDisabledAsync())
            {
                // In the scenario where Auth is disabled, we still want to have the 
                // IsAuthenticated flag set in the ClaimsIdentity. To do this requires
                // adding in an empty claim. 
                // More details on this found at:
                //  https://leastprivilege.com/2012/09/24/claimsidentity-isauthenticated-and-authenticationtype-in-net-4-5/
                //  https://stackoverflow.com/questions/20254796/why-is-my-claimsidentity-isauthenticated-always-false-for-web-api-authorize-fil
                
                ClaimsIdentity anonymousAuthenticatedIdentity = 
                        new ClaimsIdentity(new List<Claim>(), "anonymousAuth");                                 

                return anonymousAuthenticatedIdentity;
            }

            if (string.IsNullOrWhiteSpace(authHeader))
            {
                throw new UnauthorizedAccessException();
            }

            // Extract identity from token
            var tokenExtractor = new JwtTokenExtractor(
                JwtConfig.ToBotFromChannelTokenValidationParameters,
                JwtConfig.ToBotFromChannelOpenIdMetadataUrl,
                JwtConfig.AllowedSigningAlgorithms, null);

            var identity = await tokenExtractor.GetIdentityAsync(authHeader);
            if (identity == null || !identity.IsAuthenticated)
            {
                // No identity? If we're allowed to, fall back to MSA
                // This code path is used by the emulator
                tokenExtractor = new JwtTokenExtractor(
                    JwtConfig.ToBotFromEmulatorTokenValidationParameters,
                    JwtConfig.ToBotFromEmulatorOpenIdMetadataUrl,
                    JwtConfig.AllowedSigningAlgorithms, null);

                identity = await tokenExtractor.GetIdentityAsync(authHeader);
                if (identity == null || !identity.IsAuthenticated)
                {
                    throw new UnauthorizedAccessException();
                }
            }


            // Validate serviceUrl
            // Emulator token won't have this claim, so bypass if not present
            var serviceUrlClaim = identity.Claims.FirstOrDefault(claim => claim.Type == "serviceurl")?.Value;
            if (!string.IsNullOrWhiteSpace(serviceUrlClaim) && !string.Equals(serviceUrlClaim, serviceUrl))
            {
                throw new UnauthorizedAccessException();                
            }

            var identityAppId = identity.GetAppIdFromClaims();
            if (!await credentials.IsValidAppIdAsync(identityAppId))
            {                
                throw new UnauthorizedAccessException($"Invalid AppId passed on token: {identityAppId}");
            }
            
            return identity;
        }
    }
}
