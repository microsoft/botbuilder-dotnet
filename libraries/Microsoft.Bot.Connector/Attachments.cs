// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Rest;
    using Newtonsoft.Json;

    /// <summary>
    /// Attachments operations.
    /// </summary>
    public partial class Attachments : IServiceOperations<ConnectorClient>, IAttachments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Attachments"/> class.
        /// </summary>
        /// <param name='client'>
        /// Reference to the service client.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public Attachments(ConnectorClient client)
        {
            if (client == null)
            {
                throw new System.ArgumentNullException(nameof(client));
            }

            Client = client;
        }

        /// <summary>
        /// Gets a reference to the ConnectorClient.
        /// </summary>
        /// <value>The ClientConnector.</value>
        public ConnectorClient Client { get; private set; }

        /// <summary>
        /// GetAttachmentInfo.
        /// </summary>
        /// <remarks>
        /// Get AttachmentInfo structure describing the attachment views.
        /// </remarks>
        /// <param name='attachmentId'>
        /// attachment id.
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <exception cref="System.ArgumentNullException"> Thrown when a required parameter is null. </exception>
        /// <returns>
        /// A response object containing the response body and response headers.
        /// </returns>
        public async Task<HttpOperationResponse<AttachmentInfo>> GetAttachmentInfoWithHttpMessagesAsync(string attachmentId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (attachmentId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "attachmentId");
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("attachmentId", attachmentId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "GetAttachmentInfo", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "v3/attachments/{attachmentId}").ToString();
            url = url.Replace("{attachmentId}", System.Uri.EscapeDataString(attachmentId));

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
            var result = new HttpOperationResponse<AttachmentInfo>();
            result.Request = httpRequest;
            result.Response = httpResponse;

            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<AttachmentInfo>(responseContent, Client.DeserializationSettings);
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

        /// <summary>
        /// GetAttachment.
        /// </summary>
        /// <remarks>
        /// Get the named view as binary content.
        /// </remarks>
        /// <param name='attachmentId'>
        /// attachment id.
        /// </param>
        /// <param name='viewId'>
        /// View id from attachmentInfo.
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern of the data field.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">Thrown when a required parameter is null.</exception>
        /// <returns>
        /// A response object containing the response body and response headers.
        /// </returns>
        public async Task<HttpOperationResponse<Stream>> GetAttachmentWithHttpMessagesAsync(string attachmentId, string viewId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (attachmentId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "attachmentId");
            }

            if (viewId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "viewId");
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("attachmentId", attachmentId);
                tracingParameters.Add("viewId", viewId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "GetAttachment", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", StringComparison.InvariantCulture) ? string.Empty : "/")), "v3/attachments/{attachmentId}/views/{viewId}").ToString();
            url = url.Replace("{attachmentId}", System.Uri.EscapeDataString(attachmentId));
            url = url.Replace("{viewId}", System.Uri.EscapeDataString(viewId));

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
            httpResponse = await Client.HttpClient.SendAsync(httpRequest, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (shouldTrace)
            {
                ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
            }

            HttpStatusCode statusCode = httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string responseContent = null;
            if ((int)statusCode != 200 && (int)statusCode != 301 && (int)statusCode != 302)
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
            var result = new HttpOperationResponse<Stream>();
            result.Request = httpRequest;
            result.Response = httpResponse;

            // Deserialize Response
            if ((int)statusCode == 200)
            {
                result.Body = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }

            if (shouldTrace)
            {
                ServiceClientTracing.Exit(invocationId, result);
            }

            return result;
        }
    }
}
