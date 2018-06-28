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

        private static HttpClient _httpClient = new HttpClient();

        private static readonly IDictionary<string, DateTime> _trustedHostNames = new Dictionary<string, DateTime>()
        {
            // { "state.botframework.com", DateTime.MaxValue }, // deprecated state api
            { "api.botframework.com", DateTime.MaxValue},       // bot connector API
            { "token.botframework.com", DateTime.MaxValue }     // oauth token endpoint
        };


        protected static readonly Dictionary<string, Task<OAuthResponse>> tokenTaskCache = new Dictionary<string, Task<OAuthResponse>>();
        protected static readonly ConcurrentDictionary<string, DateTime> autoRefreshTimes = new ConcurrentDictionary<string, DateTime>();
        protected static readonly ConcurrentDictionary<string, OAuthResponse> tokenCache = new ConcurrentDictionary<string, OAuthResponse>();
        protected readonly string TokenCacheKey;

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


        /// <summary>
        /// TimeWindow which controls how often the token will be automatically updated
        /// </summary>
        public static TimeSpan AutoTokenRefreshTimeSpan { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Adds the host of service url to <see cref="MicrosoftAppCredentials"/> trusted hosts.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <remarks>If expiration time is not provided, the expiration time will DateTime.UtcNow.AddDays(1).</remarks>
        public static void TrustServiceUrl(string serviceUrl)
        {
            TrustServiceUrl(serviceUrl, DateTime.UtcNow.Add(TimeSpan.FromDays(1)));
        }

        /// <summary>
        /// Adds the host of service url to <see cref="MicrosoftAppCredentials"/> trusted hosts.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="expirationTime">The expiration time after which this service url is not trusted anymore.</param>
        public static void TrustServiceUrl(string serviceUrl, DateTime expirationTime)
        {
            lock (_trustedHostNames)
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
                string token = await this.GetTokenAsync().ConfigureAwait(false);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            await base.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetTokenAsync(bool forceRefresh = false)
        {
            Task<OAuthResponse> oAuthTokenTask = null;
            OAuthResponse oAuthToken = null;

            // get tokenTask from cache 
            lock (tokenTaskCache)
            {
                // if we are being forced or don't have a token in our cache at all
                if (forceRefresh || !tokenCache.TryGetValue(TokenCacheKey, out oAuthToken))
                {
                    // we will await this task, because we don't have a token and we need it
                    oAuthTokenTask = _getCurrentTokenTask(forceRefresh: forceRefresh);
                }
                else
                {
                    // we have an oAuthToken
                    // check to see if our token is expired 
                    if (IsTokenExpired(oAuthToken))
                    {
                        // it is, we should await the current task (someone could have already asked for a new token)
                        oAuthTokenTask = _getCurrentTokenTask(forceRefresh: false);

                        // if the task is completed and is the expired token, then we need to force a new one 
                        // (This happens if bot has been 100% idle past the expiration point)
                        if (oAuthTokenTask.Status == TaskStatus.RanToCompletion && oAuthTokenTask.Result.access_token == oAuthToken.access_token)
                        {
                            oAuthTokenTask = _getCurrentTokenTask(forceRefresh: true);
                        }
                    }

                    // always check the autorefresh
                    CheckAutoRefreshToken();
                }
            }

            // if we have an oAuthTokenTask then we need to await it
            if (oAuthTokenTask != null)
            {
                oAuthToken = await oAuthTokenTask;
                tokenCache[TokenCacheKey] = oAuthToken;
            }

            return oAuthToken?.access_token;
        }

        private void CheckAutoRefreshToken()
        {
            // get the current autorefreshTime for this key
            DateTime refreshTime;
            if (autoRefreshTimes.TryGetValue(TokenCacheKey, out refreshTime))
            {
                // if we are past the refresh time
                if (DateTime.UtcNow > refreshTime)
                {
                    // set new refresh time (this keeps only one outstanding refresh Task at a time)
                    autoRefreshTimes[TokenCacheKey] = DateTime.UtcNow + AutoTokenRefreshTimeSpan;

                    // background task to refresh the token
                    // NOTE: This is not awaited, but is observed with the ContinueWith() clause.  
                    // It is gated by the AutoRefreshTimes[] array
                    RefreshTokenAsync()
                        .ContinueWith(task =>
                        {
                            // observe the background task and put in cache when done
                            if (task.Status == TaskStatus.RanToCompletion)
                            {
                                // update the cache with completed task so all new requests will get it
                                tokenTaskCache[TokenCacheKey] = task;
                            }
                            else
                            {
                                // it failed, shorten the refresh time for another task to try again in a 30s
                                autoRefreshTimes[TokenCacheKey] = DateTime.UtcNow + TimeSpan.FromSeconds(30);
                            }
                        });
                }
            }
        }

        /// <summary>
        /// Do not call this except from GetTokenRefreshAsync() 
        /// </summary>
        /// <returns></returns>
        private Task<OAuthResponse> _getCurrentTokenTask(bool forceRefresh)
        {
            Task<OAuthResponse> oAuthTokenTask;

            // if there is not a task or we are forcing it
            if (forceRefresh || tokenTaskCache.TryGetValue(TokenCacheKey, out oAuthTokenTask) == false)
            {
                // create it
                oAuthTokenTask = RefreshTokenAsync();
                tokenTaskCache[TokenCacheKey] = oAuthTokenTask;

                // set initial refresh time
                autoRefreshTimes[TokenCacheKey] = DateTime.UtcNow + AutoTokenRefreshTimeSpan;
            }
            // if task is in faulted or canceled state then replace it with another attempt
            else if (oAuthTokenTask.IsFaulted || oAuthTokenTask.IsCanceled)
            {
                oAuthTokenTask = RefreshTokenAsync();
                tokenTaskCache[TokenCacheKey] = oAuthTokenTask;

                // set initial refresh time
                autoRefreshTimes[TokenCacheKey] = DateTime.UtcNow + AutoTokenRefreshTimeSpan;
            }

            return oAuthTokenTask;
        }

        private bool ShouldSetToken(HttpRequestMessage request)
        {
            if (IsTrustedUrl(request.RequestUri))
            {
                return true;
            }

            System.Diagnostics.Debug.WriteLine($"Service url {request.RequestUri.Authority} is not trusted and JwtToken cannot be sent to it.");
            return false;
        }

        private static bool IsTrustedUrl(Uri uri)
        {
            lock (_trustedHostNames)
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

        /// <summary>
        /// Has the token expired?  If so, then we await on every attempt to get a new token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool IsTokenExpired(OAuthResponse token)
        {
            return DateTime.UtcNow > token.expiration_time;
        }

        /// <summary>
        /// has token reached half/life ? If so, we get more agressive about trying to refresh it in the background
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool IsTokenOld(OAuthResponse token)
        {
            var halfwayExpiration = (token.expiration_time - TimeSpan.FromSeconds(token.expires_in / 2));
            return DateTime.UtcNow > halfwayExpiration;
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
