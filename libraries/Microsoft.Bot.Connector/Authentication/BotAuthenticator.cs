// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Bot.Connector
{
    public sealed class BotAuthenticator
    {
        /// <summary>
        /// The endorsements validator delegate. 
        /// </summary>
        /// <param name="activities"> The activities.</param>
        /// <param name="endorsements"> The endorsements used for validation.</param>
        /// <returns>true if validation passes; false otherwise.</returns>
        public delegate bool EndorsementsValidator(IEnumerable<IActivity> activities, string[] endorsements);

        private readonly ICredentialProvider credentialProvider;
        private readonly string openIdConfigurationUrl;
        private readonly bool disableEmulatorTokens;
        private readonly EndorsementsValidator validator;

        /// <summary>
        /// Creates an instance of bot authenticator. 
        /// </summary>
        /// <param name="microsoftAppId"> The Microsoft app Id.</param>
        /// <param name="microsoftAppPassword"> The Microsoft app password.</param>
        /// <param name="validator"> The endorsements validator.</param>
        /// <remarks> This constructor sets the <see cref="openIdConfigurationUrl"/> to 
        /// <see cref="JwtConfig.ToBotFromChannelOpenIdMetadataUrl"/>  and doesn't disable 
        /// the self issued tokens used by emulator.
        /// </remarks>
        public BotAuthenticator(string microsoftAppId, string microsoftAppPassword, EndorsementsValidator validator = null)
            : this(new StaticCredentialProvider(microsoftAppId, microsoftAppPassword), validator)
        {
        }

        public BotAuthenticator(ICredentialProvider credentialProvider, EndorsementsValidator validator = null)
            : this(credentialProvider, JwtConfig.ToBotFromChannelOpenIdMetadataUrl, false, validator)
        {
        }

        public BotAuthenticator(ICredentialProvider credentialProvider,
            string openIdConfigurationUrl,
            bool disableEmulatorTokens,
            EndorsementsValidator validator = null)
        {
            this.credentialProvider = credentialProvider ?? throw new ArgumentNullException("credentialProvider");
            this.openIdConfigurationUrl = openIdConfigurationUrl;
            this.disableEmulatorTokens = disableEmulatorTokens;
            this.validator = validator ?? DefaultEndorsementsValidator;
        }

        /// <summary>
        /// Generates <see cref="HttpStatusCode.Unauthorized"/> response for the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="reason">The reason phrase for unauthorized status code.</param>
        /// <returns>A response with status code unauthorized.</returns>
        public static HttpResponseMessage GenerateUnauthorizedResponse(HttpRequestMessage request, string reason = "")
        {
            string host = request.RequestUri.DnsSafeHost;
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.Headers.Add("WWW-Authenticate", string.Format("Bearer realm=\"{0}\"", host));
            if (!string.IsNullOrEmpty(reason))
            {
                response.Content = new StringContent(reason, System.Text.Encoding.UTF8);
            }
            return response;
        }

        /// <summary>
        /// Authenticates the incoming request and add the <see cref="IActivity.ServiceUrl"/> for each
        /// activities to <see cref="MicrosoftAppCredentials.TrustedHostNames"/> if the request is authenticated.
        /// </summary>
        /// <param name="request"> The request that should be authenticated.</param>
        /// <param name="activities"> The activities extracted from request.</param>
        /// <param name="token"> The cancellation token.</param>
        /// <returns></returns>
        public async Task<bool> TryAuthenticateAsync(HttpRequestMessage request, IEnumerable<IActivity> activities,
            CancellationToken token)
        {
            var identityToken = await this.AuthenticateAsync(request, activities, token);
            return identityToken.Authenticated;
        }

        /// <summary>
        /// Authenticates the request based on headers and add the <see cref="IActivity.ServiceUrl"/> for each
        /// activities to <see cref="MicrosoftAppCredentials.TrustedHostNames"/> if the request is authenticated.
        /// </summary>
        /// <param name="headers"> The headers from incoming request.</param>
        /// <param name="activities"> The activities extracted from request.</param>
        /// <param name="token"> The cancellation token.</param>
        /// <returns></returns>
        public async Task<bool> TryAuthenticateAsync(IDictionary<string, StringValues> headers, IEnumerable<IActivity> activities,
            CancellationToken token)
        {
            var request = new HttpRequestMessage();
            var authorization = StringValues.Empty;
            if (headers.Keys.Contains("Authorization") || headers.Keys.Contains("authorization"))
            {
                authorization = headers.Keys.Contains("Authorization") ? headers["Authorization"] : headers["authorization"];
            }
            request.Headers.Add("Authorization", authorization.ToArray());
            var identityToken = await this.AuthenticateAsync(request, activities, token);
            return identityToken.Authenticated;
        }

        /// <summary>
        /// Authenticates the request and returns the IdentityToken.
        /// </summary>
        /// <param name="request"> The request that should be authenticated.</param>
        /// <param name="activities"> The activities extracted from request.</param>
        /// <param name="token"> The cancellation token.</param>
        /// <returns> The <see cref="IdentityToken"/>.</returns>
        public async Task<IdentityToken> AuthenticateAsync(HttpRequestMessage request, IEnumerable<IActivity> activities,
            CancellationToken token)
        {
            var identityToken = await this.TryAuthenticateAsyncWithActivity(request, activities, token);
            identityToken.ValidateServiceUrlClaim(activities);
            TrustServiceUrls(identityToken, activities);
            return identityToken;
        }

        public async Task<IdentityToken> TryAuthenticateAsync(string scheme, string token,
            CancellationToken cancellationToken)
        {
            // then auth is disabled
            if (await this.credentialProvider.IsAuthenticationDisabledAsync())
            {
                return new IdentityToken(true, null);
            }

            var toBotFromChannel = GetTokenExtractor(JwtConfig.ToBotFromChannelTokenValidationParameters, this.openIdConfigurationUrl);
            var toBotFromEmulator = GetToBotFromEmulatorTokenExtractor();
            return await TryAuthenticateAsync(toBotFromChannel, toBotFromEmulator, scheme, token, cancellationToken);
        }

        private static bool DefaultEndorsementsValidator(IEnumerable<IActivity> activities, string[] endorsements)
        {
            return !activities.Select(activity => activity.ChannelId).Except(endorsements).Any();
        }

        private void TrustServiceUrls(IdentityToken identityToken, IEnumerable<IActivity> activities)
        {
            // add the service url to the list of trusted urls only if the JwtToken 
            // is valid and identity is not null
            if (identityToken.Authenticated && identityToken.Identity != null)
            {
                if (activities.Any())
                {
                    foreach (var activity in activities)
                    {
                        MicrosoftAppCredentials.TrustServiceUrl(activity?.ServiceUrl);
                    }
                }
                else
                {
                    //BotServiceProvider.Instance.CreateLogger().LogWarning("No ServiceUrls added to trusted list");
                }
            }
        }

        private async Task<IdentityToken> TryAuthenticateAsyncWithActivity(HttpRequestMessage request,
            IEnumerable<IActivity> activities,
            CancellationToken token)
        {
            var authorizationHeader = request.Headers.Authorization;
            if (authorizationHeader != null)
            {
                var toBotFromChannelExtractor = GetTokenExtractor(JwtConfig.ToBotFromChannelTokenValidationParameters, this.openIdConfigurationUrl, endorsements => this.validator(activities, endorsements));
                return await TryAuthenticateAsync(toBotFromChannelExtractor, GetToBotFromEmulatorTokenExtractor(), authorizationHeader.Scheme, authorizationHeader.Parameter, token);
            }
            else if (await this.credentialProvider.IsAuthenticationDisabledAsync())
            {
                return new IdentityToken(true, null);
            }

            return new IdentityToken(false, null);
        }

        private async Task<IdentityToken> TryAuthenticateAsync(JwtTokenExtractor toBotFromChannelExtractor,
            JwtTokenExtractor toBotFromEmulatorExtractor,
            string scheme,
            string token,
            CancellationToken cancellationToken)
        {
            // then auth is disabled
            if (await this.credentialProvider.IsAuthenticationDisabledAsync())
            {
                return new IdentityToken(true, null);
            }

            ClaimsIdentity identity = null;
            string appId = null;
            identity = await toBotFromChannelExtractor.GetIdentityAsync(scheme, token);
            if (identity != null)
                appId = toBotFromChannelExtractor.GetAppIdFromClaimsIdentity(identity);

            // No identity? If we're allowed to, fall back to MSA
            // This code path is used by the emulator
            if (identity == null && !this.disableEmulatorTokens)
            {
                identity = await toBotFromEmulatorExtractor.GetIdentityAsync(scheme, token);

                if (identity != null)
                    appId = toBotFromEmulatorExtractor.GetAppIdFromEmulatorClaimsIdentity(identity);
            }

            if (identity != null)
            {
                if (await credentialProvider.IsValidAppIdAsync(appId) == false) // keep context
                {
                    // not valid appid, drop the identity
                    identity = null;
                }
                else
                {
                    var password = await credentialProvider.GetAppPasswordAsync(appId); // Keep context
                    if (password != null)
                    {
                        // add password as claim so that it is part of ClaimsIdentity and accessible by ConnectorClient() 
                        identity.AddClaim(new Claim(ClaimsIdentityEx.AppPasswordClaim, password));
                    }
                }
            }

            if (identity != null)
            {
                return new IdentityToken(true, identity);
            }

            return new IdentityToken(false, null);

        }

        private static JwtTokenExtractor GetToBotFromEmulatorTokenExtractor()
        {
            return GetTokenExtractor(JwtConfig.ToBotFromEmulatorTokenValidationParameters, JwtConfig.ToBotFromEmulatorOpenIdMetadataUrl);
        }

        private static JwtTokenExtractor GetTokenExtractor(TokenValidationParameters parameters,
            string openIdConfigurationUrl,
            JwtTokenExtractor.EndorsementsValidator validator = null)
        {
            return new JwtTokenExtractor(parameters, openIdConfigurationUrl, JwtConfig.ToBotFromChannelAllowedSigningAlgorithms, validator);
        }

    }

    public sealed class IdentityToken
    {
        public readonly bool Authenticated;
        public readonly ClaimsIdentity Identity;

        public IdentityToken(bool authenticated, ClaimsIdentity identity)
        {
            this.Authenticated = authenticated;
            this.Identity = identity;
        }
    }

    public static class IdentityTokenExtensions
    {
        public static void ValidateServiceUrlClaim(this IdentityToken token, IEnumerable<IActivity> activities)
        {
            // if token is authenticated, the service url in the activities need to be validated using
            // the service url claim.
            if (token.Authenticated)
            {
                var serviceUrlClaim = token.Identity?.Claims.FirstOrDefault(claim => claim.Type == "serviceurl");

                // if there is a service url claim in the identity claims, check if it matches the service url in the activities
                if (serviceUrlClaim != null && !string.IsNullOrEmpty(serviceUrlClaim.Value))
                {
                    var filteredActivities = activities.Where(activity => string.Compare(activity.ServiceUrl, serviceUrlClaim.Value) != 0);
                    if (filteredActivities.Count() != 0)
                    {
                        throw new ArgumentException($"ServiceUrl claim: {serviceUrlClaim.Value} didn't match activity's ServiceUrl: {string.Join(",", filteredActivities.Select(activity => activity.ServiceUrl))}");
                    }
                }
            }
        }
    }
}
