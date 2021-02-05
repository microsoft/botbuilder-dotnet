// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Rest;
    using Newtonsoft.Json;

    /// <summary> UserToken operations. </summary>
    public partial class UserToken : IServiceOperations<OAuthClient>, IUserToken
    {
        /// <summary> Initializes a new instance of the <see cref="UserToken"/> class. </summary>
        /// <param name='client'> Reference to the service client. </param>
        /// <exception cref="System.ArgumentNullException"> Thrown when a required parameter is null. </exception>
        public UserToken(OAuthClient client)
        {
            if (client == null)
            {
                throw new System.ArgumentNullException(nameof(client));
            }

            Client = client;
        }

        /// <summary> Gets a reference to the OAuthClient. </summary>
        /// <value> The OAuth client. </value>
        public OAuthClient Client { get; private set; }

        /// <summary> Get token with HTTP message. </summary>
        /// <param name='userId'> User ID. </param>
        /// <param name='connectionName'> Connection name. </param>
        /// <param name='channelId'> Channel ID. </param>
        /// <param name='code'> Code. </param>
        /// <param name='customHeaders'> Headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token. </param>
        /// <exception cref="ErrorResponseException"> Thrown when the operation returned an invalid status code. </exception>
        /// <exception cref="SerializationException"> Thrown when unable to deserialize the response. </exception>
        /// <exception cref="ValidationException"> Thrown when a required parameter is null. </exception>
        /// <exception cref="System.ArgumentNullException"> Thrown when an input value does not match the expected data type, range or pattern. </exception>
        /// <returns> A response object containing the response body and response headers. </returns>
        public async Task<HttpOperationResponse<TokenResponse>> GetTokenWithHttpMessagesAsync(string userId, string connectionName, string channelId = default(string), string code = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (userId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "userId");
            }

            if (connectionName == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "connectionName");
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("userId", userId);
                tracingParameters.Add("connectionName", connectionName);
                tracingParameters.Add("channelId", channelId);
                tracingParameters.Add("code", code);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "GetToken", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "api/usertoken/GetToken").ToString();
            List<string> queryParameters = new List<string>();
            if (userId != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "userId={0}", System.Uri.EscapeDataString(userId)));
            }

            if (connectionName != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "connectionName={0}", System.Uri.EscapeDataString(connectionName)));
            }

            if (channelId != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "channelId={0}", System.Uri.EscapeDataString(channelId)));
            }

            if (code != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "code={0}", System.Uri.EscapeDataString(code)));
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
            if ((int)statusCode != 200 && (int)statusCode != 404)
            {
                var ex = new ErrorResponseException($"Operation returned an invalid status code '{statusCode}'");
                try
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ErrorResponse errorBody = Rest.Serialization.SafeJsonConvert.DeserializeObject<ErrorResponse>(responseContent, Client.DeserializationSettings);
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

                httpRequest.Dispose();
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }

                throw ex;
            }

            // Create Result
            var result = new HttpOperationResponse<TokenResponse>();
            result.Request = httpRequest;
            result.Response = httpResponse;

            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<TokenResponse>(responseContent, Client.DeserializationSettings);
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

            // Deserialize Response
            if ((int)statusCode == 404)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<TokenResponse>(responseContent, Client.DeserializationSettings);
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

        /// <summary> Get AAD Tokens with HTTP messages. </summary>
        /// <param name='userId'> User ID. </param>
        /// <param name='connectionName'> Connection name. </param>
        /// <param name='aadResourceUrls'> AAD resource URLs. </param>
        /// <param name='channelId'> Channel ID. </param>
        /// <param name='customHeaders'> Headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token. </param>
        /// <exception cref="ErrorResponseException"> Thrown when the operation returned an invalid status code. </exception>
        /// <exception cref="SerializationException"> Thrown when unable to deserialize the response. </exception>
        /// <exception cref="ValidationException"> Thrown when a required parameter is null. </exception>
        /// <exception cref="System.ArgumentNullException"> Thrown when an input value does not match the expected data type, range or pattern. </exception>
        /// <returns> A response object containing the response body and response headers. </returns>
        public async Task<HttpOperationResponse<IDictionary<string, TokenResponse>>> GetAadTokensWithHttpMessagesAsync(string userId, string connectionName, AadResourceUrls aadResourceUrls, string channelId = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (userId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "userId");
            }

            if (connectionName == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "connectionName");
            }

            if (aadResourceUrls == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "aadResourceUrls");
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("userId", userId);
                tracingParameters.Add("connectionName", connectionName);
                tracingParameters.Add("aadResourceUrls", aadResourceUrls);
                tracingParameters.Add("channelId", channelId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "GetAadTokens", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "api/usertoken/GetAadTokens").ToString();
            List<string> queryParameters = new List<string>();
            if (userId != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "userId={0}", System.Uri.EscapeDataString(userId)));
            }

            if (connectionName != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "connectionName={0}", System.Uri.EscapeDataString(connectionName)));
            }

            if (channelId != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "channelId={0}", System.Uri.EscapeDataString(channelId)));
            }

            if (queryParameters.Count > 0)
            {
                url += "?" + string.Join("&", queryParameters);
            }

            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("POST");
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
            if (aadResourceUrls != null)
            {
                requestContent = Rest.Serialization.SafeJsonConvert.SerializeObject(aadResourceUrls, Client.SerializationSettings);
                httpRequest.Content = new StringContent(requestContent, System.Text.Encoding.UTF8);
                httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }

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
                var ex = new ErrorResponseException($"Operation returned an invalid status code '{statusCode}'");
                try
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ErrorResponse errorBody = Rest.Serialization.SafeJsonConvert.DeserializeObject<ErrorResponse>(responseContent, Client.DeserializationSettings);
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

                httpRequest.Dispose();
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }

                throw ex;
            }

            // Create Result
            var result = new HttpOperationResponse<IDictionary<string, TokenResponse>>();
            result.Request = httpRequest;
            result.Response = httpResponse;

            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<IDictionary<string, TokenResponse>>(responseContent, Client.DeserializationSettings);
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

        /// <summary>Sign out with HTTP message. </summary>
        /// <param name='userId'> User ID. </param>
        /// <param name='connectionName'> Connection name. </param>
        /// <param name='channelId'> Channel ID.</param>
        /// <param name='customHeaders'> Headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token. </param>
        /// <exception cref="ErrorResponseException"> Thrown when the operation returned an invalid status code. </exception>
        /// <exception cref="SerializationException"> Thrown when unable to deserialize the response. </exception>
        /// <exception cref="ValidationException"> Thrown when a required parameter is null. </exception>
        /// <exception cref="System.ArgumentNullException"> Thrown when an input value does not match the expected data type, range or pattern. </exception>
        /// <returns> A response object containing the response body and response headers. </returns>
        public async Task<HttpOperationResponse<object>> SignOutWithHttpMessagesAsync(string userId, string connectionName = default(string), string channelId = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (userId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "userId");
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("userId", userId);
                tracingParameters.Add("connectionName", connectionName);
                tracingParameters.Add("channelId", channelId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "SignOut", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "api/usertoken/SignOut").ToString();
            List<string> queryParameters = new List<string>();
            if (userId != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "userId={0}", System.Uri.EscapeDataString(userId)));
            }

            if (connectionName != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "connectionName={0}", System.Uri.EscapeDataString(connectionName)));
            }

            if (channelId != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "channelId={0}", System.Uri.EscapeDataString(channelId)));
            }

            if (queryParameters.Count > 0)
            {
                url += "?" + string.Join("&", queryParameters);
            }

            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("DELETE");
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
            if ((int)statusCode != 200 && (int)statusCode != 204)
            {
                var ex = new ErrorResponseException($"Operation returned an invalid status code '{statusCode}'");
                try
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ErrorResponse errorBody = Rest.Serialization.SafeJsonConvert.DeserializeObject<ErrorResponse>(responseContent, Client.DeserializationSettings);
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

                httpRequest.Dispose();
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }

                throw ex;
            }

            // Create Result
            var result = new HttpOperationResponse<object>();
            result.Request = httpRequest;
            result.Response = httpResponse;

            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<object>(responseContent, Client.DeserializationSettings);
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

        /// <summary> Get Token Status with HTTP message. </summary>
        /// <param name='userId'> User ID. </param>
        /// <param name='channelId'> Channel ID. </param>
        /// <param name='include'> Include. </param>
        /// <param name='customHeaders'> Headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token. </param>
        /// <exception cref="ErrorResponseException"> Thrown when the operation returned an invalid status code. </exception>
        /// <exception cref="SerializationException"> Thrown when unable to deserialize the response. </exception>
        /// <exception cref="ValidationException"> Thrown when an input value does not match the expected data type, range or pattern. </exception>
        /// <exception cref="System.ArgumentNullException"> Thrown when a required parameter is null. </exception>
        /// <returns> A response object containing the response body and response headers. </returns>
        public async Task<HttpOperationResponse<IList<TokenStatus>>> GetTokenStatusWithHttpMessagesAsync(string userId, string channelId = default(string), string include = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (userId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "userId");
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("userId", userId);
                tracingParameters.Add("channelId", channelId);
                tracingParameters.Add("include", include);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "GetTokenStatus", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "api/usertoken/GetTokenStatus").ToString();
            List<string> queryParameters = new List<string>();
            if (userId != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "userId={0}", System.Uri.EscapeDataString(userId)));
            }

            if (channelId != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "channelId={0}", System.Uri.EscapeDataString(channelId)));
            }

            if (include != null)
            {
                queryParameters.Add(string.Format(CultureInfo.InvariantCulture, "include={0}", System.Uri.EscapeDataString(include)));
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
                var ex = new ErrorResponseException($"Operation returned an invalid status code '{statusCode}'");
                try
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ErrorResponse errorBody = Rest.Serialization.SafeJsonConvert.DeserializeObject<ErrorResponse>(responseContent, Client.DeserializationSettings);
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

                httpRequest.Dispose();
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }

                throw ex;
            }

            // Create Result
            var result = new HttpOperationResponse<IList<TokenStatus>>();
            result.Request = httpRequest;
            result.Response = httpResponse;

            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<IList<TokenStatus>>(responseContent, Client.DeserializationSettings);
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
