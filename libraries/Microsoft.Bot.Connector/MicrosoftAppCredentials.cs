// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// MicrosoftAppCredentials auth implementation and cache
    /// </summary>
    public class MicrosoftAppCredentials : ServiceClientCredentials
    {
        /// <summary>
        /// The key for Microsoft app Id.
        /// </summary>
        public const string MicrosoftAppIdKey = "MicrosoftAppId";

        /// <summary>
        /// The key for Microsoft app Password.
        /// </summary>
        public const string MicrosoftAppPasswordKey = "MicrosoftAppPassword";

        // TODO: Complete list with proper TrustedHostNames
        protected static ConcurrentDictionary<string, DateTime> TrustedHostNames = new ConcurrentDictionary<string, DateTime>(
                                                                                        new Dictionary<string, DateTime>() {
                                                                                            { "state.botframework.com", DateTime.MaxValue }
                                                                                        });

        protected static readonly ConcurrentDictionary<string, OAuthResponse> cache = new ConcurrentDictionary<string, OAuthResponse>();

        public MicrosoftAppCredentials(string appId, string password)
        {
            this.MicrosoftAppId = appId;

            this.MicrosoftAppPassword = password;

            this.TokenCacheKey = $"{MicrosoftAppId}-cache";
        }

        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppPassword { get; set; }

        public virtual string OAuthEndpoint { get { return JwtConfig.ToChannelFromBotLoginUrl; } }
        public virtual string OAuthScope { get { return JwtConfig.ToChannelFromBotOAuthScope; } }

        protected readonly string TokenCacheKey;

        /// <summary>
        /// Adds the host of service url to <see cref="MicrosoftAppCredentials"/> trusted hosts.
        /// </summary>
        /// <param name="serviceUrl">The service url</param>
        /// <param name="expirationTime">The expiration time after which this service url is not trusted anymore</param>
        /// <remarks>If expiration time is not provided, the expiration time will DateTime.UtcNow.AddDays(1).</remarks>
        public static void TrustServiceUrl(string serviceUrl, DateTime expirationTime = default(DateTime))
        {
            try
            {
                if (expirationTime == default(DateTime))
                {
                    // by default the service url is valid for one day
                    var extensionPeriod = TimeSpan.FromDays(1);
                    TrustedHostNames.AddOrUpdate(new Uri(serviceUrl).Host, DateTime.UtcNow.Add(extensionPeriod), (key, oldValue) =>
                    {
                        var newExpiration = DateTime.UtcNow.Add(extensionPeriod);
                        // try not to override expirations that are greater than one day from now
                        if (oldValue > newExpiration)
                        {
                            // make sure that extension can be added to oldValue and ArgumentOutOfRangeException
                            // is not thrown
                            if (oldValue >= DateTime.MaxValue.Subtract(extensionPeriod))
                            {
                                newExpiration = oldValue;
                            }
                            else
                            {
                                newExpiration = oldValue.Add(extensionPeriod);
                            }
                        }
                        return newExpiration;
                    });
                }
                else
                {
                    TrustedHostNames.AddOrUpdate(new Uri(serviceUrl).Host, expirationTime, (key, oldValue) => expirationTime);
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Checks if the service url is for a trusted host or not.
        /// </summary>
        /// <param name="serviceUrl">The service url</param>
        /// <returns>True if the host of the service url is trusted; False otherwise.</returns>
        public static bool IsTrustedServiceUrl(string serviceUrl)
        {
            Uri uri;
            if (Uri.TryCreate(serviceUrl, UriKind.Absolute, out uri))
            {
                return TrustedUri(uri);
            }
            return false;
        }

        /// <summary>
        /// Apply the credentials to the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param><param name="cancellationToken">Cancellation token.</param>
        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ShouldSetToken(request))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
            }
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }



        public async Task<string> GetTokenAsync(bool forceRefresh = false)
        {
            string token;
            OAuthResponse oAuthToken;
            if (cache.TryGetValue(TokenCacheKey, out oAuthToken) && !forceRefresh && TokenNotExpired(oAuthToken))
            {
                token = oAuthToken.access_token;
            }
            else
            {
                oAuthToken = await RefreshTokenAsync().ConfigureAwait(false);
                cache.AddOrUpdate(TokenCacheKey, oAuthToken, (key, oldToken) => oAuthToken);
                token = oAuthToken.access_token;
            }
            return token;
        }

        private bool ShouldSetToken(HttpRequestMessage request)
        {
            if (TrustedUri(request.RequestUri))
            {
                return true;
            }

            return false;
        }

        private static bool TrustedUri(Uri uri)
        {
            DateTime trustedServiceUrlExpiration;
            if (TrustedHostNames.TryGetValue(uri.Host, out trustedServiceUrlExpiration))
            {
                // check if the trusted service url is still valid
                if (trustedServiceUrlExpiration > DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)))
                {
                    return true;
                }
            }
            return false;
        }

        public sealed class OAuthException : Exception
        {
            public OAuthException(string body, Exception inner)
                : base(body, inner)
            {
            }
        }

        private async Task<OAuthResponse> RefreshTokenAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", MicrosoftAppId },
                    { "client_secret", MicrosoftAppPassword },
                    { "scope", OAuthScope }
                });

                using (var response = await httpClient.PostAsync(OAuthEndpoint, content).ConfigureAwait(false))
                {
                    string body = null;
                    try
                    {
                        response.EnsureSuccessStatusCode();
                        body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var oauthResponse = JsonConvert.DeserializeObject<OAuthResponse>(body);
                        oauthResponse.expiration_time = DateTime.UtcNow.AddSeconds(oauthResponse.expires_in).Subtract(TimeSpan.FromSeconds(60));
                        return oauthResponse;
                    }
                    catch (Exception error)
                    {
                        throw new OAuthException(body ?? response.ReasonPhrase, error);
                    }
                }
            }
        }

        private bool TokenNotExpired(OAuthResponse token)
        {
            return token.expiration_time > DateTime.UtcNow;
        }

        protected class OAuthResponse
        {
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string access_token { get; set; }
            public DateTime expiration_time { get; set; }
        }
    }
}
