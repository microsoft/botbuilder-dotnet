// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;

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
        /// Get a Facebook API client with the correct credentials based on the page identified in the incoming activity.
        /// This is used by many internal functions to get access to the Facebook API, and is exposed as `bot.api` on any BotWorker instances passed into Botkit handler functions.
        /// </summary>
        /// <param name="activity">An incoming message activity.</param>
        /// <returns>A Facebook API client.</returns>
        public async Task<FacebookClientWrapper> GetAPIAsync(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (!string.IsNullOrWhiteSpace(Options.AccessToken))
            {
                return new FacebookClientWrapper(new FacebookAdapterOptions(Options.VerifyToken, Options.AppSecret, Options.AccessToken));
            }

            if (string.IsNullOrWhiteSpace(activity.Recipient?.Id))
            {
                throw new Exception($"Unable to create API based on activity:{activity}");
            }

            var pageId = activity.Recipient.Id;

            if ((activity.ChannelData as dynamic)?.message != null && (activity.ChannelData as dynamic)?.message.is_echo)
            {
                pageId = activity.From.Id;
            }

            var token = await Options.GetAccessTokenForPageAsync(pageId).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException(nameof(token));
            }

            return new FacebookClientWrapper(new FacebookAdapterOptions(Options.VerifyToken, Options.AppSecret, token));
        }

        /// <summary>
        /// Verifies the SHA1 signature of the raw request payload before bodyParser parses it will abort parsing if signature is invalid, and pass a generic error to response.
        /// </summary>
        /// <param name="request">An Http request object.</param>
        /// <param name="response">An Http response object.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> VerifySignatureAsync(HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var expected = request.Headers["x-hub-signature"];

            string calculated = null;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Options.AppSecret)))
            {
                using (var bodyStream = new StreamReader(request.Body))
                {
                    calculated = $"sha1={hmac.ComputeHash(Encoding.UTF8.GetBytes(bodyStream.ReadToEnd()))}";
                }
            }

            if (expected != calculated)
            {
                response.StatusCode = Convert.ToInt32(HttpStatusCode.Unauthorized, System.Globalization.CultureInfo.InvariantCulture);
                response.ContentType = "text/plain";
                await response.WriteAsync(string.Empty, cancellationToken).ConfigureAwait(false);
                return false;
            }

            return true;
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
