// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Connector.Authentication
{
    public static class JwtTokenValidation
    {
        private static HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Authenticates the request and sets the service url in the set of trusted urls.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="credentials">The credentials.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns>ClaimsIdentity for the request.</returns>
        public static async Task<ClaimsIdentity> AuthenticateRequest(IActivity activity, string authHeader, ICredentialProvider credentials, HttpClient httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                bool isAuthDisabled = await credentials.IsAuthenticationDisabledAsync();
                if (isAuthDisabled)
                {
                    // In the scenario where Auth is disabled, we still want to have the 
                    // IsAuthenticated flag set in the ClaimsIdentity. To do this requires
                    // adding in an empty claim. 
                    return new ClaimsIdentity(new List<Claim>(), "anonymous");
                }

                // No Auth Header. Auth is required. Request is not authorized. 
                throw new UnauthorizedAccessException();
            }

            var claimsIdentity = await ValidateAuthHeader(authHeader, credentials, activity.ServiceUrl, httpClient ?? _httpClient);

            MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);

            return claimsIdentity;
        }

        public static async Task<ClaimsIdentity> ValidateAuthHeader(string authHeader, ICredentialProvider credentials, string serviceUrl = null, HttpClient httpClient = null)
        {
            if (string.IsNullOrEmpty(authHeader)) throw new ArgumentNullException(nameof(authHeader));

            bool usingEmulator = EmulatorValidation.IsTokenFromEmulator(authHeader);

            if (usingEmulator)
            {
                return await EmulatorValidation.AuthenticateEmulatorToken(authHeader, credentials, httpClient ?? _httpClient);
            }
            else
            {
                // No empty or null check. Empty can point to issues. Null checks only.
                if (serviceUrl != null)
                {
                    return await ChannelValidation.AuthenticateChannelToken(authHeader, credentials, serviceUrl, httpClient ?? _httpClient);
                }
                else
                {
                    return await ChannelValidation.AuthenticateChannelToken(authHeader, credentials, httpClient ?? _httpClient);
                }
            }
        }
    }
}
