// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Contains helper methods for authenticating incoming HTTP requests.
    /// </summary>
    public static class JwtTokenValidation
    {
        private static HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Authenticates the request and add's the activity's <see cref="Activity.ServiceUrl"/>
        /// to the set of trusted URLs.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="credentials">The bot's credential provider.</param>
        /// <param name="provider">The bot's channel service provider.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the claims-based
        /// identity for the request.</remarks>
        public static async Task<ClaimsIdentity> AuthenticateRequest(IActivity activity, string authHeader, ICredentialProvider credentials, IChannelProvider provider, HttpClient httpClient = null)
        {
            return await AuthenticateRequest(activity, authHeader, credentials, provider, new AuthenticationConfiguration(), httpClient);
        }

        /// <summary>
        /// Authenticates the request and add's the activity's <see cref="Activity.ServiceUrl"/>
        /// to the set of trusted URLs.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="credentials">The bot's credential provider.</param>
        /// <param name="provider">The bot's channel service provider.</param>
        /// <param name="authConfig">The optional authentication configuration.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the claims-based
        /// identity for the request.</remarks>
        public static async Task<ClaimsIdentity> AuthenticateRequest(IActivity activity, string authHeader, ICredentialProvider credentials, IChannelProvider provider, AuthenticationConfiguration authConfig, HttpClient httpClient = null)
        {
            if (authConfig == null)
            {
                throw new ArgumentNullException(nameof(authConfig));
            }

            if (string.IsNullOrWhiteSpace(authHeader))
            {
                bool isAuthDisabled = await credentials.IsAuthenticationDisabledAsync().ConfigureAwait(false);
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

            var claimsIdentity = await ValidateAuthHeader(authHeader, credentials, provider, activity.ChannelId, authConfig, activity.ServiceUrl, httpClient ?? _httpClient);

            MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);

            return claimsIdentity;
        }

        /// <summary>
        /// Validates the authentication header of an incoming request.
        /// </summary>
        /// <param name="authHeader">The authentication header to validate.</param>
        /// <param name="credentials">The bot's credential provider.</param>
        /// <param name="channelProvider">The bot's channel service provider.</param>
        /// <param name="channelId">The ID of the channel that sent the request.</param>
        /// <param name="serviceUrl">The service URL for the activity.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the claims-based
        /// identity for the request.</remarks>
        public static async Task<ClaimsIdentity> ValidateAuthHeader(string authHeader, ICredentialProvider credentials, IChannelProvider channelProvider, string channelId, string serviceUrl = null, HttpClient httpClient = null)
        {
            return await ValidateAuthHeader(authHeader, credentials, channelProvider, channelId, new AuthenticationConfiguration(), serviceUrl, httpClient).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates the authentication header of an incoming request.
        /// </summary>
        /// <param name="authHeader">The authentication header to validate.</param>
        /// <param name="credentials">The bot's credential provider.</param>
        /// <param name="channelProvider">The bot's channel service provider.</param>
        /// <param name="channelId">The ID of the channel that sent the request.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="serviceUrl">The service URL for the activity.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the claims-based
        /// identity for the request.</remarks>
        public static async Task<ClaimsIdentity> ValidateAuthHeader(string authHeader, ICredentialProvider credentials, IChannelProvider channelProvider, string channelId, AuthenticationConfiguration authConfig, string serviceUrl = null, HttpClient httpClient = null)
        {
            if (string.IsNullOrEmpty(authHeader))
            {
                throw new ArgumentNullException(nameof(authHeader));
            }

            if (authConfig == null)
            {
                throw new ArgumentNullException(nameof(authConfig));
            }

            httpClient = httpClient ?? _httpClient;

            bool usingEmulator = EmulatorValidation.IsTokenFromEmulator(authHeader);

            if (usingEmulator)
            {
                return await EmulatorValidation.AuthenticateEmulatorToken(authHeader, credentials, channelProvider, httpClient, channelId, authConfig);
            }
            else if (channelProvider == null || channelProvider.IsPublicAzure())
            {
                // No empty or null check. Empty can point to issues. Null checks only.
                if (serviceUrl != null)
                {
                    return await ChannelValidation.AuthenticateChannelToken(authHeader, credentials, serviceUrl, httpClient, channelId, authConfig);
                }
                else
                {
                    return await ChannelValidation.AuthenticateChannelToken(authHeader, credentials, httpClient, channelId, authConfig);
                }
            }
            else if (channelProvider.IsGovernment())
            {
                return await GovernmentChannelValidation.AuthenticateChannelToken(authHeader, credentials, serviceUrl, httpClient, channelId, authConfig).ConfigureAwait(false);
            }
            else
            {
                return await EnterpriseChannelValidation.AuthenticateChannelToken(authHeader, credentials, channelProvider, serviceUrl, httpClient, channelId, authConfig).ConfigureAwait(false);
            }
        }
    }
}
