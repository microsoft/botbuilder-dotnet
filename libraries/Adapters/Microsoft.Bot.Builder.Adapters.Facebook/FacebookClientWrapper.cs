// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    public class FacebookClientWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookClientWrapper"/> class.
        /// </summary>
        /// <param name="options">An object containing API credentials, a webhook verification token and other options.</param>
        public FacebookClientWrapper(FacebookAdapterOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the FacebookAdapterOptions.
        /// </summary>
        /// <value>
        /// An object containing API credentials, a webhook verification token and other options.
        /// </value>
        public FacebookAdapterOptions Options { get; private set; }

        /// <summary>
        /// Call one of the Facebook APIs.
        /// </summary>
        /// <param name="path">Path to the API endpoint, for example `/me/messages`.</param>
        /// <param name="payload">An object to be sent as parameters to the API call..</param>
        /// <param name="method">HTTP method, for example POST, GET, DELETE or PUT.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A task representing the async operation.</returns>
        public async Task<HttpResponseMessage> CallAPIAsync(string path, FacebookMessage payload, HttpMethod method = null, CancellationToken cancellationToken = default)
        {
            var proof = GetAppSecretProof(Options.AccessToken, Options.AppSecret);

            if (method == null)
            {
                method = HttpMethod.Post;
            }

            // send the request
            using (var request = new HttpRequestMessage())
            {
                request.RequestUri = new Uri($"https://{Options.ApiHost}/{Options.ApiVersion + path}?access_token={Options.AccessToken}&appsecret_proof={proof}");
                request.Method = method;

                /* content type json? */

                using (var client = new HttpClient())
                {
                    return await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Generate the app secret proof used to increase security on calls to the graph API.
        /// </summary>
        /// <param name="accessToken">A page access token.</param>
        /// <param name="appSecret">An app secret.</param>
        /// <returns>The app secret proof.</returns>
        private string GetAppSecretProof(string accessToken, string appSecret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret)))
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(accessToken)).ToString();
            }
        }
    }
}
