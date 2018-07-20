// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    /// <summary>
    /// Wraps an HTTP client with which to obtain an access token.
    /// </summary>
    internal class AzureAuthToken
    {
        // Name of header used to pass the subscription key to the token service
        private const string OcpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";

        private static readonly HttpClient DefaultHttpClient = new HttpClient();

        // URL of the token service
        private static readonly Uri ServiceUrl = new Uri("https://api.cognitive.microsoft.com/sts/v1.0/issueToken");

        // After obtaining a valid token, this class will cache it for this duration.
        // Use a duration of 5 minutes, which is less than the actual token lifetime of 10 minutes.
        private static readonly TimeSpan TokenCacheDuration = new TimeSpan(0, 5, 0);

        private HttpClient _httpClient = null;

        // Cache the value of the last valid token obtained from the token service.
        private string _storedTokenValue = string.Empty;

        // When the last valid token was obtained.
        private DateTime _storedTokenTime = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAuthToken"/> class.
        /// </summary>
        /// <param name="key">Subscription key to use to get an authentication token.</param>
        /// <param name="httpClient">An alternate HTTP client to use.</param>
        internal AzureAuthToken(string key, HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? DefaultHttpClient;
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "A subscription key is required");
            }

            this.SubscriptionKey = key;
            this.RequestStatusCode = HttpStatusCode.InternalServerError;
        }

        // Gets the subscription key.
        internal string SubscriptionKey { get; }

        // Gets the HTTP status code for the most recent request to the token service.
        internal HttpStatusCode RequestStatusCode { get; private set; }

        /// <summary>
        /// Gets a token for the specified subscription.
        /// </summary>
        /// <returns>The encoded JWT token prefixed with the string "Bearer ".</returns>
        /// <remarks>
        /// This method uses a cache to limit the number of request to the token service.
        /// A fresh token can be re-used during its lifetime of 10 minutes. After a successful
        /// request to the token service, this method caches the access token. Subsequent
        /// invocations of the method return the cached token for the next 5 minutes. After
        /// 5 minutes, a new token is fetched from the token service and the cache is updated.
        /// </remarks>
        internal async Task<string> GetAccessTokenAsync()
        {
            if (string.IsNullOrWhiteSpace(this.SubscriptionKey))
            {
                return string.Empty;
            }

            // Re-use the cached token if there is one.
            if ((DateTime.Now - _storedTokenTime) < TokenCacheDuration)
            {
                return _storedTokenValue;
            }

            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = ServiceUrl;
                request.Content = new StringContent(string.Empty);
                request.Headers.TryAddWithoutValidation(OcpApimSubscriptionKeyHeader, this.SubscriptionKey);
                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    this.RequestStatusCode = response.StatusCode;
                    response.EnsureSuccessStatusCode();
                    var token = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    _storedTokenTime = DateTime.Now;
                    _storedTokenValue = "Bearer " + token;
                    return _storedTokenValue;
                }
            }
        }
    }
}
