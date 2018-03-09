// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// MicrosoftAppCredentials auth implementation and cache
    /// </summary>
    public class MicrosoftAppCredentials : ServiceClientCredentials
    {
        /// <summary>
        /// An empty set of credentials.
        /// </summary>
        public static readonly MicrosoftAppCredentials Empty = new MicrosoftAppCredentials(null, null);

        /// <summary>
        /// The key for Microsoft app Id.
        /// </summary>
        public const string MicrosoftAppIdKey = "MicrosoftAppId";

        /// <summary>
        /// The key for Microsoft app Password.
        /// </summary>
        public const string MicrosoftAppPasswordKey = "MicrosoftAppPassword";

        /// <summary>
        /// The token refresh code uses this client. Ideally, this would be passed in or set via a DI system to 
        /// allow developer control over behavior / headers / timeouts and such. Unfortunately this is buried
        /// pretty deep, the static solution used here is much cleaner. If this becomes an issue we could
        /// consider circling back and exposing developer control over this HttpClient. 
        /// </summary>
        private static readonly HttpClient _httpClient = new HttpClient(); 

        private static object _trustedHostNamesSync = new object();
        private static readonly IDictionary<string, DateTime> _trustedHostNames = new Dictionary<string, DateTime>()
        {
            { "state.botframework.com", DateTime.MaxValue }
        };

        private static object _cacheSync = new object();
        protected static readonly IDictionary<string, OAuthResponse> _cache = new Dictionary<string, OAuthResponse>();

        public MicrosoftAppCredentials(string appId, string password)
        {
            this.MicrosoftAppId = appId;
            this.MicrosoftAppPassword = password;
            this.TokenCacheKey = $"{MicrosoftAppId}-cache";            
        }

        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppPassword { get; set; }

        public virtual string OAuthEndpoint { get { return AuthenticationConstants.ToChannelFromBotLoginUrl; } }
        public virtual string OAuthScope { get { return AuthenticationConstants.ToChannelFromBotOAuthScope; } }

        protected readonly string TokenCacheKey;

        /// <summary>
        /// Adds the host of service url to <see cref="MicrosoftAppCredentials"/> trusted hosts.
        /// </summary>
        /// <param name="serviceUrl">The service url</param>
        /// <param name="expirationTime">The expiration time after which this service url is not trusted anymore</param>
        /// <remarks>If expiration time is not provided, the expiration time will DateTime.UtcNow.AddDays(1).</remarks>
        public static void TrustServiceUrl(string serviceUrl)
        {
            TrustServiceUrl(serviceUrl, DateTime.UtcNow.Add(TimeSpan.FromDays(1)));
        }

        public static void TrustServiceUrl(string serviceUrl, DateTime expirationTime)
        {
            lock (_trustedHostNamesSync)
            {
                _trustedHostNames[new Uri(serviceUrl).Host] = expirationTime;
            }
        }

        /// <summary>
        /// Checks if the service url is for a trusted host or not.
        /// </summary>
        /// <param name="serviceUrl">The service url</param>
        /// <returns>True if the host of the service url is trusted; False otherwise.</returns>
        public static bool IsTrustedServiceUrl(string serviceUrl)
        {
            if (Uri.TryCreate(serviceUrl, UriKind.Absolute, out Uri uri))
            {
                return IsTrustedUrl(uri);
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
            if (forceRefresh == false)
            {
                // check the global cache for the token. If we have it, and it's valid, we're done. 
                lock (_cacheSync)
                {
                    bool foundToken = _cache.TryGetValue(TokenCacheKey, out OAuthResponse oAuthToken);
                    if (foundToken)
                    {
                        // we have the token. Is it valid? 
                        if (oAuthToken.expiration_time > DateTime.UtcNow)
                        {
                            return oAuthToken.access_token;
                        }
                    }
                }
            }

            // We need to refresh the token, because:
            // 1. The user requested it via the forceRefresh parameter
            // 2. We have it, but it's expired
            // 3. We don't have it in the cache. 

            OAuthResponse token = await RefreshTokenAsync().ConfigureAwait(false);
            lock (_cacheSync)
            {
                _cache[TokenCacheKey] = token;
            }

            return token.access_token;
        }

        private bool ShouldSetToken(HttpRequestMessage request)
        {
            if (IsTrustedUrl(request.RequestUri))
            {
                return true;
            }

            return false;
        }

        private static bool IsTrustedUrl(Uri uri)
        {
            lock (_trustedHostNamesSync)
            {
                if (_trustedHostNames.TryGetValue(uri.Host, out DateTime trustedServiceUrlExpiration))
                {
                    // check if the trusted service url is still valid
                    if (trustedServiceUrlExpiration > DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)))
                    {
                        return true;
                    }
                }
                return false;
            }
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
            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", MicrosoftAppId },
                    { "client_secret", MicrosoftAppPassword },
                    { "scope", OAuthScope }
                });

            using (var response = await _httpClient.PostAsync(OAuthEndpoint, content).ConfigureAwait(false))
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

#pragma warning disable IDE1006
        /// <remarks>
        /// Member variables to this class follow the RFC Naming conventions, rather than C# naming conventions. 
        /// </remarks>
        protected class OAuthResponse
        {
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string access_token { get; set; }
            public DateTime expiration_time { get; set; }
        }
#pragma warning restore IDE1006
    }
}
