// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Authentication helpers.
    /// </summary>
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Call context storage to propagate values throught the request.
        /// </summary>
        private static AsyncLocal<BotFrameworkAuthenticationContext> asyncLocal = new AsyncLocal<BotFrameworkAuthenticationContext>();

        /// <summary>
        /// Gets the request authentication context async.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns></returns>
        public static async Task<BotFrameworkAuthenticationContext> GetRequestAuthenticationContextAsync(string authHeader, HttpClient httpClient)
        {
            ClaimsIdentity claimsIdentity;

            if (string.IsNullOrEmpty(authHeader))
            {
                claimsIdentity = new ClaimsIdentity(new List<Claim>(), "anonymous");
            }

            bool usingEmulator = EmulatorValidation.IsTokenFromEmulator(authHeader);

            if (usingEmulator)
            {
                claimsIdentity = await EmulatorValidation.GetEmulatorTokenIdentityAsync(authHeader, httpClient);
            }
            else
            {
                claimsIdentity = await ChannelValidation.GetChannelTokenIdentityAsync(authHeader, httpClient);
            }

            // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For 
            // unauthenticated requests we have anonymouse identity provided auth is disabled.
            var botAppIdClaim = (claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim)
                ??
                // For Activities coming from Emulator AppId claim contains the Bot's AAD AppId.
                claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AppIdClaim));

            var serviceUrlClaim = claimsIdentity.Claims.FirstOrDefault(claim => claim.Type == AuthenticationConstants.ServiceUrlClaim);

            // Why not set directly to the AsyncLocal you ask? Well the way AsyncLocal works is that the threads spawned from this thread
            // get the data. So you want to set the data on current thread for the children to get. But we are returning here, there are no 
            // children. So if we were to set the AsyncLocal here. It will just get lost in the wind.
            return new BotFrameworkAuthenticationContext
            {
                ClaimsIdentity = claimsIdentity,
                BotAppId = botAppIdClaim == null ? null : botAppIdClaim.Value,
                ServiceUrl = serviceUrlClaim == null ? null : serviceUrlClaim.Value,
                IsEmulator = usingEmulator,
                IsAuthenticated = null
            };
        }

        /// <summary>
        /// Sets the request authentication context.
        /// </summary>
        public static void SetRequestAuthenticationContext(BotFrameworkAuthenticationContext authenticationContext)
        {
            asyncLocal.Value = authenticationContext;
        }

        /// <summary>
        /// Gets the request context.
        /// </summary>
        /// <returns>Request context.</returns>
        public static BotFrameworkAuthenticationContext GetBotFrameworkAuthenticationContext()
        {
            try
            {
                return asyncLocal.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
