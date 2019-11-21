// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    public class FacebookClientWrapper
    {
        /// <summary>
        /// An instance of the FacebookAdapterOptions class.
        /// </summary>
        private readonly FacebookAdapterOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookClientWrapper"/> class.
        /// </summary>
        /// <param name="options">An object containing API credentials, a webhook verification token and other options.</param>
        public FacebookClientWrapper(FacebookAdapterOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Send a REST message to Facebook.
        /// </summary>
        /// <param name="path">Path to the API endpoint, for example `/me/messages`.</param>
        /// <param name="payload">An object to be sent as parameters to the API call..</param>
        /// <param name="method">HTTP method, for example POST, GET, DELETE or PUT.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A task representing the async operation.</returns>
        public virtual async Task<string> SendMessageAsync(string path, FacebookMessage payload, HttpMethod method = null, CancellationToken cancellationToken = default)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var proof = GetAppSecretProof();

            if (method == null)
            {
                method = HttpMethod.Post;
            }

            using (var request = new HttpRequestMessage())
            {
#pragma warning disable CA1308 // the appsecret_proof should be sent in lowerCase.
                request.RequestUri = new Uri($"https://{_options.FacebookApiHost}/{_options.FacebookApiVersion + path}?access_token={_options.FacebookAccessToken}&appsecret_proof={proof.ToLowerInvariant()}");
#pragma warning restore CA1308 // the appsecret_proof should be sent in lowerCase.
                request.Method = method;
                var json = JsonConvert.SerializeObject(
                    payload,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                    });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var res = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    if (res.IsSuccessStatusCode)
                    {
                        var responseBody = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var stringResponse = JsonConvert.DeserializeObject<FacebookResponseOk>(responseBody);
                        return stringResponse.MessageId;
                    }

                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Verifies the SHA1 signature of the raw request payload before bodyParser parses it will abort parsing if signature is invalid, and pass a generic error to response.
        /// </summary>
        /// <param name="request">An Http request object.</param>
        /// <param name="payload">The request body.</param>
        /// <returns>The result of the comparison between the signature in the request and hashed body.</returns>
        public virtual bool VerifySignature(HttpRequest request, string payload)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var expected = request.Headers["x-hub-signature"].ToString().ToUpperInvariant();

#pragma warning disable CA5350 // Facebook uses SHA1 as cryptographic algorithm.
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_options.FacebookAppSecret)))
            {
                hmac.Initialize();
                var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var hash = $"SHA1={BitConverter.ToString(hashArray).Replace("-", string.Empty)}";

                return expected == hash;
            }
#pragma warning restore CA5350 // Facebook uses SHA1 as cryptographic algorithm.
        }

        /// <summary>
        /// Generate the app secret proof used to increase security on calls to the graph API.
        /// </summary>
        /// <returns>The app secret proof.</returns>
        public virtual string GetAppSecretProof()
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.FacebookAppSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(_options.FacebookAccessToken));
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        /// <summary>
        /// Verifies the VerifyToken from the message and if it matches the one configured, it sends back the challenge.
        /// </summary>
        /// <param name="request">An Http request object.</param>
        /// <param name="response">An Http response object.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A task representing the async operation.</returns>
        public virtual async Task VerifyWebhookAsync(HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var challenge = string.Empty;
            HttpStatusCode statusCode;

            if (request.Query["hub.verify_token"].Equals(_options.FacebookVerifyToken))
            {
                challenge = request.Query["hub.challenge"];
                statusCode = HttpStatusCode.OK;
            }
            else
            {
                statusCode = HttpStatusCode.Unauthorized;
            }

            await FacebookHelper.WriteAsync(response, statusCode, challenge, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Posts webhook control events to Facebook.
        /// </summary>
        /// <param name="postType">The REST post type (GET, PUT, POST, etc).</param>
        /// <param name="content">The string content to be posted to Facebook.</param>
        /// <returns>A bool indicating the success of the operation.</returns>
        public virtual async Task<bool> PostToFacebookApiAsync(string postType, string content)
        {
            if (postType == null)
            {
                throw new ArgumentNullException(nameof(postType));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var graphApiBaseUrl = $"https://{_options.FacebookApiHost}/{_options.FacebookApiVersion + postType}?access_token={_options.FacebookAccessToken}";
            var requestPath = string.Format(System.Globalization.CultureInfo.InvariantCulture, graphApiBaseUrl, postType, _options.FacebookAccessToken);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            using (var requestMessage = new HttpRequestMessage())
            {
                requestMessage.Method = new HttpMethod("POST");
                requestMessage.RequestUri = new Uri(requestPath);
                requestMessage.Content = stringContent;
                requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

                using (var client = new HttpClient())
                {
                    var res = await client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(false);
                    return res.IsSuccessStatusCode;
                }
            }
        }

        /// <summary>
        /// Sends the request_thread_control webhook event to Facebook.
        /// </summary>
        /// <param name="userId">The sender user Id.</param>
        /// <param name="message">An optional message for the metadata paremter.</param>
        /// <returns>A bool value indicating the success of the operation.</returns>
        public virtual async Task<bool> RequestThreadControlAsync(string userId, string message)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var content = new { recipient = new { id = userId }, metadata = message };
            return await PostToFacebookApiAsync($"/me/{HandoverConstants.RequestThreadControl}", JsonConvert.SerializeObject(content)).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the take_thread_control webhook event to Facebook.
        /// </summary>
        /// <param name="userId">The sender user Id.</param>
        /// <param name="message">An optional message for the metadata paremter.</param>
        /// <returns>A bool value indicating the success of the operation.</returns>
        public virtual async Task<bool> TakeThreadControlAsync(string userId, string message)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var content = new { recipient = new { id = userId }, metadata = message };
            return await PostToFacebookApiAsync($"/me/{HandoverConstants.TakeThreadControl}", JsonConvert.SerializeObject(content)).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the pass_thread_control webhook event to Facebook.
        /// </summary>
        /// <param name="targetAppId">The Id of the target app to pass control to.</param>
        /// <param name="userId">The sender user Id.</param>
        /// <param name="message">An optional message for the metadata paremter.</param>
        /// <returns>A bool value indicating the success of the operation.</returns>
        public virtual async Task<bool> PassThreadControlAsync(string targetAppId, string userId, string message)
        {
            if (targetAppId == null)
            {
                throw new ArgumentNullException(nameof(targetAppId));
            }

            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var content = new { recipient = new { id = userId }, target_app_id = targetAppId, metadata = message };
            return await PostToFacebookApiAsync($"/me/{HandoverConstants.PassThreadControl}", JsonConvert.SerializeObject(content)).ConfigureAwait(false);
        }
    }
}
