// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Connector.Authentication
{
    public static class JwtTokenValidation
    {       
        /// <summary>
        /// Validates the security tokens required by the Bot Framework Protocol. Throws on any exceptions. 
        /// </summary>
        /// <param name="activity">The incoming Activity from the Bot Framework or the Emulator</param>
        /// <param name="authHeader">The Bearer token included as part of the request</param>
        /// <param name="credentials">The set of valid credentials, such as the Bot Application ID</param>
        /// <returns>Nothing</returns>
        public static async Task AssertValidActivity(Activity activity, string authHeader, ICredentialProvider credentials)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                // No auth header was sent. We might be on the anonymous code path. 
                bool isAuthDisabled = await credentials.IsAuthenticationDisabledAsync();
                if (isAuthDisabled)
                {
                    // We are on the anonymous code path. 
                    return;
                }
            }

            // Go through the standard authentication path. 
            await JwtTokenValidation.ValidateAuthHeader(authHeader, credentials, activity.ServiceUrl);

            // On the standard Auth path, we need to trust the URL that was incoming. 
            MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
        }

        public static async Task<ClaimsIdentity> ValidateAuthHeader(string authHeader, ICredentialProvider credentials)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                bool isAuthDisabled = await credentials.IsAuthenticationDisabledAsync();
                if (isAuthDisabled)
                {
                    // In the scenario where Auth is disabled, we still want to have the 
                    // IsAuthenticated flag set in the ClaimsIdentity. To do this requires
                    // adding in an empty claim. 
                    ClaimsIdentity anonymousAuthenticatedIdentity = new ClaimsIdentity(new List<Claim>(), "anonymous");
                    return anonymousAuthenticatedIdentity;
                }

                // No Auth Header. Auth is required. Request is not authorized. 
                throw new UnauthorizedAccessException();
            }

            bool usingEmulator = EmulatorValidation.IsTokenFromEmulator(authHeader);
            if (usingEmulator)
            {
                return await EmulatorValidation.AuthenticateEmulatorToken(authHeader, credentials);
            }
            else
            {
                return await ChannelValidation.AuthenticateChannelToken(authHeader, credentials);
            }
        }

        public static async Task<ClaimsIdentity> ValidateAuthHeader(string authHeader, ICredentialProvider credentials, string serviceUrl)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                bool isAuthDisabled = await credentials.IsAuthenticationDisabledAsync();
                if (isAuthDisabled)
                {
                    // In the scenario where Auth is disabled, we still want to have the 
                    // IsAuthenticated flag set in the ClaimsIdentity. To do this requires
                    // adding in an empty claim. 
                    ClaimsIdentity anonymousAuthenticatedIdentity = new ClaimsIdentity(new List<Claim>(), "anonymous");
                    return anonymousAuthenticatedIdentity;
                }

                // No Auth Header. Auth is required. Request is not authorized. 
                throw new UnauthorizedAccessException();
            }

            bool usingEmulator = EmulatorValidation.IsTokenFromEmulator(authHeader);
            if (usingEmulator)
            {
                return await EmulatorValidation.AuthenticateEmulatorToken(authHeader, credentials);
            }
            else
            {
                return await ChannelValidation.AuthenticateChannelToken(authHeader, credentials, serviceUrl);
            }
        }             
    }
}
