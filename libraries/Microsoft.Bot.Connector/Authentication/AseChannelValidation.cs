// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Validates and Examines JWT tokens from the AseChannel.
    /// </summary>
    [Obsolete("Use `ConfigurationBotFrameworkAuthentication` instead to perform AseChannel validation.", false)]
    public static class AseChannelValidation
    {
        /// <summary>
        /// Just used for app service extension v2 (independent app service).
        /// </summary>
        public const string ChannelId = "AseChannel";

        /// <summary>
        /// TO BOT FROM AseChannel: Token validation parameters when connecting to a channel.
        /// </summary>
        public static readonly TokenValidationParameters ToBotFromAseChannelTokenValidationParameters =
            new TokenValidationParameters()
            {
                ValidateIssuer = true,

                // Audience validation takes place manually in code.
                ValidateAudience = false, // lgtm[cs/web/missing-token-validation]
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
            };

        private static string _metadataUrl;
        private static ICredentialProvider _defaultCredentialProvider;
        private static IChannelProvider _defaultChannelProvider;
        private static HttpClient _authHttpClient = new HttpClient();

        /// <summary>
        /// Set up user issue/metadataUrl for AseChannel validation.
        /// </summary>
        /// <param name="configuration">App Configurations, will GetSection MicrosoftAppId/MicrosoftAppTenantId/ChannelService/ToBotFromAseOpenIdMetadataUrl.</param>
        public static void Init(IConfiguration configuration)
        {
            var appId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            var tenantId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppTenantIdKey)?.Value;
 
            var channelService = configuration.GetSection("ChannelService")?.Value;
            var toBotFromAseOpenIdMetadataUrl = configuration.GetSection("ToBotFromAseOpenIdMetadataUrl")?.Value;

            _defaultCredentialProvider = new SimpleCredentialProvider(appId, string.Empty);
            _defaultChannelProvider = new SimpleChannelProvider(channelService);

            _metadataUrl = !string.IsNullOrEmpty(toBotFromAseOpenIdMetadataUrl)
                ? toBotFromAseOpenIdMetadataUrl
                : (_defaultChannelProvider.IsGovernment()
                    ? GovernmentAuthenticationConstants.ToBotFromAseChannelOpenIdMetadataUrl
                    : AuthenticationConstants.ToBotFromAseChannelOpenIdMetadataUrl);

            var tenantIds = new string[]
            {
                tenantId,
                "f8cdef31-a31e-4b4a-93e4-5f571e91255a", // US Gov MicrosoftServices.onmicrosoft.us
                "d6d49420-f39b-4df7-a1dc-d59a935871db" // Public botframework.com
            };

            var validIssuers = new HashSet<string>();
            foreach (var tmpId in tenantIds)
            {
                validIssuers.Add($"https://sts.windows.net/{tmpId}/"); // Auth Public/US Gov, 1.0 token
                validIssuers.Add($"https://login.microsoftonline.com/{tmpId}/v2.0"); // Auth Public, 2.0 token
                validIssuers.Add($"https://login.microsoftonline.us/{tmpId}/v2.0"); // Auth for US Gov, 2.0 token
            }

            ToBotFromAseChannelTokenValidationParameters.ValidIssuers = validIssuers;
        }

        /// <summary>
        /// Determines if a request from AseChannel.
        /// </summary>
        /// <param name="channelId">need to be same with ChannelId.</param>
        /// <returns>True, if the token was issued by the AseChannel. Otherwise, false.</returns>
        public static bool IsAseChannel(string channelId)
        {
            return channelId == ChannelId;            
        }        

        /// <summary>
        /// Validate the incoming Auth Header as a token sent from the AseChannel.
        /// </summary>
        /// <param name="authHeader">The raw HTTP header in the format: "Bearer [longString]".</param>
        /// <param name="credentials">The user defined set of valid credentials, such as the AppId.</param>
        /// <param name="httpClient">Authentication of tokens requires calling out to validate Endorsements and related documents. The
        /// HttpClient is used for making those calls. Those calls generally require TLS connections, which are expensive to
        /// setup and teardown, so a shared HttpClient is recommended.</param>
        /// <returns>
        /// A valid ClaimsIdentity.
        /// </returns>
        public static async Task<ClaimsIdentity> AuthenticateAseTokenAsync(
            string authHeader, 
            ICredentialProvider credentials = default, 
            HttpClient httpClient = default)
        {
            credentials = credentials ?? _defaultCredentialProvider;
            httpClient = httpClient ?? _authHttpClient;

            return await AuthenticateAseTokenAsync(authHeader, credentials, httpClient, new AuthenticationConfiguration()).ConfigureAwait(false);
        }

        /// <summary>
        /// Validate the incoming Auth Header as a token sent from the AseChannel.
        /// </summary>
        /// <param name="authHeader">The raw HTTP header in the format: "Bearer [longString]".</param>
        /// <param name="credentials">The user defined set of valid credentials, such as the AppId.</param>
        /// <param name="httpClient">Authentication of tokens requires calling out to validate Endorsements and related documents. The
        /// HttpClient is used for making those calls. Those calls generally require TLS connections, which are expensive to
        /// setup and teardown, so a shared HttpClient is recommended.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <returns>
        /// A valid ClaimsIdentity.
        /// </returns>
        public static async Task<ClaimsIdentity> AuthenticateAseTokenAsync(string authHeader, ICredentialProvider credentials, HttpClient httpClient,  AuthenticationConfiguration authConfig)
        {
            if (authConfig == null)
            {
                throw new ArgumentNullException(nameof(authConfig));
            }

            var tokenExtractor = new JwtTokenExtractor(
                    httpClient,
                    ToBotFromAseChannelTokenValidationParameters,
                    _metadataUrl,
                    AuthenticationConstants.AllowedSigningAlgorithms);

            var identity = await tokenExtractor.GetIdentityAsync(authHeader, ChannelId, authConfig.RequiredEndorsements).ConfigureAwait(false);
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
                throw new UnauthorizedAccessException("'ver' claim is required on AseChannel Tokens.");
            }

            string tokenVersion = versionClaim.Value;
            string appID = string.Empty;

            // The AseChannel, depending on Version, sends the AppId via either the
            // appid claim (Version 1) or the Authorized Party claim (Version 2).
            if (string.IsNullOrWhiteSpace(tokenVersion) || tokenVersion == "1.0")
            {
                // either no Version or a version of "1.0" means we should look for
                // the claim in the "appid" claim.
                Claim appIdClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.AppIdClaim);
                if (appIdClaim == null)
                {
                    // No claim around AppID. Not Authorized.
                    throw new UnauthorizedAccessException("'appid' claim is required on AseChannel Token version '1.0'.");
                }

                appID = appIdClaim.Value;
            }
            else if (tokenVersion == "2.0")
            {
                // AseChannel, "2.0" puts the AppId in the "azp" claim.
                Claim appZClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.AuthorizedParty);
                if (appZClaim == null)
                {
                    // No claim around AppID. Not Authorized.
                    throw new UnauthorizedAccessException("'azp' claim is required on AseChannel Token version '2.0'.");
                }

                appID = appZClaim.Value;
            }
            else
            {
                // Unknown Version. Not Authorized.
                throw new UnauthorizedAccessException($"Unknown AseChannel Token version '{tokenVersion}'.");
            }

            if (!await credentials.IsValidAppIdAsync(appID).ConfigureAwait(false))
            {
                await Console.Out.WriteLineAsync(appID).ConfigureAwait(false);
            }

            return identity;
        }
    }
}
