// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System.Collections;
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

    /// <summary> An OAuth client class that implements <see cref="IOAuthClient"/>. </summary>
    public partial class OAuthClient : ServiceClient<OAuthClient>, IOAuthClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/> class.
        /// </summary>
        /// <param name='credentials'>
        /// Required. Subscription credentials which uniquely identify client subscription.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public OAuthClient(ServiceClientCredentials credentials, params DelegatingHandler[] handlers)
            : this(handlers)
        {
            if (credentials == null)
            {
                throw new System.ArgumentNullException(nameof(credentials));
            }

            Credentials = credentials;
            if (Credentials != null)
            {
                Credentials.InitializeServiceClient(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/> class.
        /// </summary>
        /// <param name='credentials'>
        /// Required. Subscription credentials which uniquely identify client subscription.
        /// </param>
        /// <param name='httpClient'>
        /// HttpClient to be used.
        /// </param>
        /// <param name='disposeHttpClient'>
        /// True: will dispose the provided httpClient on calling OAuthClient.Dispose(). False: will not dispose provided httpClient.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public OAuthClient(ServiceClientCredentials credentials, HttpClient httpClient, bool disposeHttpClient)
            : this(httpClient, disposeHttpClient)
        {
            if (credentials == null)
            {
                throw new System.ArgumentNullException(nameof(credentials));
            }

            Credentials = credentials;
            if (Credentials != null)
            {
                Credentials.InitializeServiceClient(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/> class.
        /// </summary>
        /// <param name='credentials'>
        /// Required. Subscription credentials which uniquely identify client subscription.
        /// </param>
        /// <param name='rootHandler'>
        /// Optional. The http client handler used to handle http transport.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public OAuthClient(ServiceClientCredentials credentials, HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
            : this(rootHandler, handlers)
        {
            if (credentials == null)
            {
                throw new System.ArgumentNullException(nameof(credentials));
            }

            Credentials = credentials;
            if (Credentials != null)
            {
                Credentials.InitializeServiceClient(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/> class.
        /// </summary>
        /// <param name='baseUri'>
        /// Optional. The base URI of the service.
        /// </param>
        /// <param name='credentials'>
        /// Required. Subscription credentials which uniquely identify client subscription.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public OAuthClient(System.Uri baseUri, ServiceClientCredentials credentials, params DelegatingHandler[] handlers)
            : this(handlers)
        {
            if (baseUri == null)
            {
                throw new System.ArgumentNullException(nameof(baseUri));
            }

            if (credentials == null)
            {
                throw new System.ArgumentNullException(nameof(credentials));
            }

            BaseUri = baseUri;
            Credentials = credentials;
            if (Credentials != null)
            {
                Credentials.InitializeServiceClient(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/> class.
        /// </summary>
        /// <param name='baseUri'>
        /// Optional. The base URI of the service.
        /// </param>
        /// <param name='credentials'>
        /// Required. Subscription credentials which uniquely identify client subscription.
        /// </param>
        /// <param name='rootHandler'>
        /// Optional. The http client handler used to handle http transport.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public OAuthClient(System.Uri baseUri, ServiceClientCredentials credentials, HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
            : this(rootHandler, handlers)
        {
            if (baseUri == null)
            {
                throw new System.ArgumentNullException(nameof(baseUri));
            }

            if (credentials == null)
            {
                throw new System.ArgumentNullException(nameof(credentials));
            }

            BaseUri = baseUri;
            Credentials = credentials;
            if (Credentials != null)
            {
                Credentials.InitializeServiceClient(this);
            }
        }

        /// <summary> Initializes a new instance of the <see cref="OAuthClient"/> class. </summary>
        /// <param name='httpClient'> HttpClient to be used. </param>
        /// <param name='disposeHttpClient'> True: will dispose the provided httpClient on calling OAuthClient.Dispose(). False: will not dispose provided httpClient. </param>
        protected OAuthClient(HttpClient httpClient, bool disposeHttpClient)
            : base(httpClient, disposeHttpClient)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/> class.
        /// </summary>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        protected OAuthClient(params DelegatingHandler[] handlers)
            : base(handlers)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/> class.
        /// </summary>
        /// <param name='rootHandler'>
        /// Optional. The http client handler used to handle http transport.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        protected OAuthClient(HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
            : base(rootHandler, handlers)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/> class.
        /// </summary>
        /// <param name='baseUri'>
        /// Optional. The base URI of the service.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        protected OAuthClient(System.Uri baseUri, params DelegatingHandler[] handlers)
            : this(handlers)
        {
            if (baseUri == null)
            {
                throw new System.ArgumentNullException(nameof(baseUri));
            }

            BaseUri = baseUri;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/> class.
        /// </summary>
        /// <param name='baseUri'>
        /// Optional. The base URI of the service.
        /// </param>
        /// <param name='rootHandler'>
        /// Optional. The http client handler used to handle http transport.
        /// </param>
        /// <param name='handlers'>
        /// Optional. The delegating handlers to add to the http client pipeline.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        protected OAuthClient(System.Uri baseUri, HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
            : this(rootHandler, handlers)
        {
            if (baseUri == null)
            {
                throw new System.ArgumentNullException(nameof(baseUri));
            }

            BaseUri = baseUri;
        }

        /// <summary> Gets or sets the base URI of the service. </summary>
        /// <value> The base URI. </value>
        public System.Uri BaseUri { get; set; }

        /// <summary> Gets json serialization settings. </summary>
        /// <value>The serialization settings.</value>
        public JsonSerializerSettings SerializationSettings { get; private set; }

        /// <summary> Gets json deserialization settings. </summary>
        /// <value> The deserialization settings. </value>
        public JsonSerializerSettings DeserializationSettings { get; private set; }

        /// <summary> Gets subscription credentials which uniquely identify client subscription. </summary>
        /// <value>The client credentials. </value>
        public ServiceClientCredentials Credentials { get; private set; }

        /// <summary> Gets the IBotSignIn. </summary>
        /// <value> A class that performs bot sign-in operations. </value>
        public virtual IBotSignIn BotSignIn { get; private set; }

        /// <summary> Gets the IUserToken. </summary>
        /// <value> The <see cref="UserToken"/>. </value>
        public virtual IUserToken UserToken { get; private set; }

        /// <summary>Exchange with HTTP message.</summary>
        /// <param name='userId'> User ID. </param>
        /// <param name='connectionName'> Connection name. </param>
        /// <param name='channelId'> Channel ID. </param>
        /// <param name='exchangeRequest'> Exechange request. </param>
        /// <param name='customHeaders'> Headers that will be added to request.</param>
        /// <param name='cancellationToken'> The cancellation token. </param>
        /// <exception cref="ErrorResponseException"> Thrown when the operation returned an invalid status code. </exception>
        /// <exception cref="SerializationException"> Thrown when unable to deserialize the response. </exception>
        /// <exception cref="ValidationException"> Thrown when an input value does not match the expected data type, range or pattern. </exception>
        /// <exception cref="System.ArgumentNullException"> Thrown when a required parameter is null. </exception>
        /// <return> A response object containing the response body and response headers. </return>
        /// <returns> A task that represents the work queued to execute.</returns>
        public async Task<HttpOperationResponse<object>> ExchangeAsyncWithHttpMessagesAsync(string userId, string connectionName, string channelId, TokenExchangeRequest exchangeRequest, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (userId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "userId");
            }

            if (connectionName == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "connectionName");
            }

            if (channelId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "channelId");
            }

            if (exchangeRequest == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "exchangeRequest");
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
                tracingParameters.Add("exchangeRequest", exchangeRequest);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "ExchangeAsync", tracingParameters);
            }

            // Construct URL
            var baseUrl = this.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "api/usertoken/exchange").ToString();
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
            if (exchangeRequest != null)
            {
                requestContent = Rest.Serialization.SafeJsonConvert.SerializeObject(exchangeRequest, this.SerializationSettings);
                httpRequest.Content = new StringContent(requestContent, System.Text.Encoding.UTF8);
                httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }

            // Set Credentials
            if (this.Credentials != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            }

            // Send Request
            if (shouldTrace)
            {
                ServiceClientTracing.SendRequest(invocationId, httpRequest);
            }

            cancellationToken.ThrowIfCancellationRequested();
            httpResponse = await this.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            if (shouldTrace)
            {
                ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
            }

            HttpStatusCode statusCode = httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string responseContent = null;
            if ((int)statusCode != 200 && (int)statusCode != 400 && (int)statusCode != 404)
            {
                var ex = new ErrorResponseException($"Operation returned an invalid status code '{statusCode}'");
                try
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ErrorResponse errorBody = Rest.Serialization.SafeJsonConvert.DeserializeObject<ErrorResponse>(responseContent, this.DeserializationSettings);
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
                    result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<TokenResponse>(responseContent, this.DeserializationSettings);
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
            if ((int)statusCode == 400)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<ErrorResponse>(responseContent, this.DeserializationSettings);
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
                    result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<TokenResponse>(responseContent, this.DeserializationSettings);
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

        /// <summary> Get sign-in resource with HTTP message. </summary>
        /// <param name='state'> State. </param>
        /// <param name='codeChallenge'> Code challenge. </param>
        /// <param name='emulatorUrl'> Emulator URL. </param>
        /// <param name='finalRedirect'> Final redirect. </param>
        /// <param name='customHeaders'> Headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token. </param>
        /// <exception cref="HttpOperationException"> Thrown when the operation returned an invalid status code. </exception>
        /// <exception cref="SerializationException"> Thrown when unable to deserialize the response. </exception>
        /// <exception cref="ValidationException"> Thrown when an input value does not match the expected data type, range or pattern. </exception>
        /// <exception cref="System.ArgumentNullException"> Thrown when a required parameter is null. </exception>
        /// <returns> A response object containing the response body and response headers. </returns>
        public async Task<HttpOperationResponse<SignInResource>> GetSignInResourceWithHttpMessagesAsync(string state, string codeChallenge = default(string), string emulatorUrl = default(string), string finalRedirect = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
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
                ServiceClientTracing.Enter(invocationId, this, "GetSignInResource", tracingParameters);
            }

            // Construct URL
            var baseUrl = this.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "api/botsignin/GetSignInResource").ToString();
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
            if (this.Credentials != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            }

            // Send Request
            if (shouldTrace)
            {
                ServiceClientTracing.SendRequest(invocationId, httpRequest);
            }

            cancellationToken.ThrowIfCancellationRequested();
            httpResponse = await this.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
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
            var result = new HttpOperationResponse<SignInResource>();
            result.Request = httpRequest;
            result.Response = httpResponse;

            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<SignInResource>(responseContent, this.DeserializationSettings);
                }
                catch (JsonException ex)
                {
                    if (System.Uri.TryCreate(responseContent, System.UriKind.Absolute, out _))
                    {
                        // if the response is an uri, the service contract is incorrect.
                        result.Body = new SignInResource(responseContent);
                    }
                    else
                    {
                        httpRequest.Dispose();
                        if (httpResponse != null)
                        {
                            httpResponse.Dispose();
                        }

                        throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                    }
                }
            }

            if (shouldTrace)
            {
                ServiceClientTracing.Exit(invocationId, result);
            }

            return result;
        }

        /// <summary>
        /// An optional partial-method to perform custom initialization.
        /// </summary>
        partial void CustomInitialize();

        /// <summary>
        /// Initializes client properties.
        /// </summary>
        private void Initialize()
        {
            BotSignIn = new BotSignIn(this);
            UserToken = new UserToken(this);
            BaseUri = new System.Uri(AuthenticationConstants.OAuthUrl);
            SerializationSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                    {
                        new Iso8601TimeSpanConverter()
                    }
            };
            DeserializationSettings = new JsonSerializerSettings
            {
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                    {
                        new Iso8601TimeSpanConverter()
                    }
            };
            CustomInitialize();
        }
    }
}
