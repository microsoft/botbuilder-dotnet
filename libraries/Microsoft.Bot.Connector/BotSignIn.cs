// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Rest;
    using Newtonsoft.Json;

    /// <summary>
    /// BotSignIn operations.
    /// </summary>
    public partial class BotSignIn : IServiceOperations<OAuthClient>, IBotSignIn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotSignIn"/> class.
        /// </summary>
        /// <param name='client'>
        /// Reference to the service client.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public BotSignIn(OAuthClient client)
        {
            if (client == null)
            {
                throw new System.ArgumentNullException(nameof(client));
            }

            Client = client;
        }

        /// <summary>
        /// Gets a reference to the OAuthClient.
        /// </summary>
        /// <value>The OAuthClient.</value>
        public OAuthClient Client { get; private set; }

        /// <summary>Gets sign-in URL with HTTP message. </summary>
        /// <param name='state'>State.</param>
        /// <param name='codeChallenge'>Code challenge.</param>
        /// <param name='emulatorUrl'>Emulator URL.</param>
        /// <param name='finalRedirect'>Final redirect.</param>
        /// <param name='customHeaders'>Headers that will be added to request.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <exception cref="HttpOperationException">Thrown when the operation returned an invalid status code.</exception>
        /// <exception cref="SerializationException">Thrown when unable to deserialize the response.</exception>
        /// <exception cref="ValidationException">Thrown when an input value does not match the expected data type, range or pattern of the data field.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when a required parameter is null.</exception>
        /// <returns>A response object containing the response body and response headers.</returns>
        public async Task<HttpOperationResponse<string>> GetSignInUrlWithHttpMessagesAsync(string state, string codeChallenge = default(string), string emulatorUrl = default(string), string finalRedirect = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (state == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "state");
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("state", state);
                tracingParameters.Add("codeChallenge", codeChallenge);
                tracingParameters.Add("emulatorUrl", emulatorUrl);
                tracingParameters.Add("finalRedirect", finalRedirect);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "GetSignInUrl", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "api/botsignin/GetSignInUrl").ToString();
            List<string> queryParameters = new List<string>();
            if (state != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "state={0}", System.Uri.EscapeDataString(state)));
            }

            if (codeChallenge != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "code_challenge={0}", System.Uri.EscapeDataString(codeChallenge)));
            }

            if (emulatorUrl != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "emulatorUrl={0}", System.Uri.EscapeDataString(emulatorUrl)));
            }

            if (finalRedirect != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "finalRedirect={0}", System.Uri.EscapeDataString(finalRedirect)));
            }

            if (queryParameters.Count > 0)
            {
                url += "?" + string.Join("&", queryParameters);
            }

            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = new System.Uri(url);

            // Set Headers
            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    if (httpRequest.Headers.Contains(header.Key))
                    {
                        httpRequest.Headers.Remove(header.Key);
                    }

                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Serialize Request
            string requestContent = null;

            // Set Credentials
            if (Client.Credentials != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Client.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            }

            // Send Request
            if (shouldTrace)
            {
                ServiceClientTracing.SendRequest(invocationId, httpRequest);
            }

            cancellationToken.ThrowIfCancellationRequested();
            httpResponse = await Client.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            if (shouldTrace)
            {
                ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
            }

            HttpStatusCode statusCode = httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string responseContent = null;
            if ((int)statusCode != 200)
            {
                var ex = new HttpOperationException($"Operation returned an invalid status code '{statusCode}'");
                if (httpResponse.Content != null)
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    responseContent = string.Empty;
                }

                ex.Request = new HttpRequestMessageWrapper(httpRequest, requestContent);
                ex.Response = new HttpResponseMessageWrapper(httpResponse, responseContent);
                if (shouldTrace)
                {
                    ServiceClientTracing.Error(invocationId, ex);
                }

                httpRequest.Dispose();

                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }

                throw ex;
            }

            // Create Result
            var result = new HttpOperationResponse<string>();
            result.Request = httpRequest;
            result.Response = httpResponse;

            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    // MANUAL SWAGGER UPDATE
                    result.Body = responseContent;
                }
                catch (JsonException ex)
                {
                    httpRequest.Dispose();
                    if (httpResponse != null)
                    {
                        httpResponse.Dispose();
                    }

                    throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                }
            }

            if (shouldTrace)
            {
                ServiceClientTracing.Exit(invocationId, result);
            }

            return result;
        }
    }
}
