// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// A class containing OAuthClient configuration values.
    /// </summary>
    public static class OAuthClientConfig
    {
        /// <summary>
        /// Gets or sets the default endpoint that is used for API requests.
        /// </summary>
        /// <value>
        /// The default endpoint that is used for API requests.
        /// </value>
        public static string OAuthEndpoint { get; set; } = AuthenticationConstants.OAuthUrl;

        /// <summary>
        /// Gets or sets a value indicating whether when using the Emulator, whether to emulate the OAuthCard behavior or use connected flows.
        /// </summary>
        /// <value>
        /// When using the Emulator, whether to emulate the OAuthCard behavior or use connected flows.
        /// </value>
        public static bool EmulateOAuthCards { get; set; } = false;

        /// <summary>
        /// Send a dummy OAuth card when the bot is being used on the Emulator for testing without fetching a real token.
        /// </summary>
        /// <param name="client">client.</param>
        /// <param name="emulateOAuthCards">Indicates whether the Emulator should emulate the OAuth card.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task SendEmulateOAuthCardsAsync(OAuthClient client, bool emulateOAuthCards)
        {
            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("emulateOAuthCards", emulateOAuthCards);
                ServiceClientTracing.Enter(invocationId, client, "GetToken", tracingParameters);
            }

            // Construct URL
            var baseUrl = client.BaseUri.AbsoluteUri;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase) ? string.Empty : "/")), "api/usertoken/emulateOAuthCards?emulate={emulate}").ToString();
            url = url.Replace("{emulate}", emulateOAuthCards.ToString());

            // Create HTTP transport objects
            using (var httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = new HttpMethod("POST");
                httpRequest.RequestUri = new Uri(url);

                var cancellationToken = CancellationToken.None;

                // Serialize Request
                string requestContent = null;

                // Set Credentials
                if (client.Credentials != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await client.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                }

                // Send Request
                if (shouldTrace)
                {
                    ServiceClientTracing.SendRequest(invocationId, httpRequest);
                }

                cancellationToken.ThrowIfCancellationRequested();
                using (var httpResponse = await client.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false))
                {
                    if (shouldTrace)
                    {
                        ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
                    }

                    var statusCode = httpResponse.StatusCode;
                    cancellationToken.ThrowIfCancellationRequested();
                    string responseContent = null;
                    if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NotFound)
                    {
                        var ex = new ErrorResponseException(string.Format(CultureInfo.InvariantCulture, "Operation returned an invalid status code '{0}'", statusCode));
                        try
                        {
                            responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var errorBody = SafeJsonConvert.DeserializeObject<ErrorResponse>(responseContent, client.DeserializationSettings);
                            if (errorBody != null)
                            {
                                ex.Body = errorBody;
                            }
                        }
                        catch (JsonException)
                        {
                            // Ignore the exception
                        }

                        ex.Request = new HttpRequestMessageWrapper(httpRequest, requestContent);
                        ex.Response = new HttpResponseMessageWrapper(httpResponse, responseContent);
                        if (shouldTrace)
                        {
                            ServiceClientTracing.Error(invocationId, ex);
                        }

                        throw ex;
                    }

                    if (shouldTrace)
                    {
                        // Create  and log result
                        using (var result = new HttpOperationResponse<int>())
                        {
                            result.Request = httpRequest;
                            result.Response = httpResponse;
                            ServiceClientTracing.Exit(invocationId, result);
                        }
                    }
                }
            }
        }
    }
}
