// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Helper for HTTP requests.
    /// </summary>
    public class HttpRequestHelper
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestHelper"/> class.
        /// </summary>
        /// <param name="httpClient">Http client.</param>
        public HttpRequestHelper(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        /// <summary>
        /// Execute Http request.
        /// </summary>
        /// <param name="requestUrl">Http request url.</param>
        /// <param name="payloadBody">Http request body.</param>
        /// <param name="endpoint">QnA Maker endpoint details.</param>
        /// <returns>Returns http response object.</returns>
        public async Task<HttpResponseMessage> ExecuteHttpRequest(string requestUrl, string payloadBody, QnAMakerEndpoint endpoint)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            request.Content = new StringContent(payloadBody, Encoding.UTF8, "application/json");

            SetHeaders(request, endpoint);

            var response = await this._httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return response;
        }

        private void SetHeaders(HttpRequestMessage request, QnAMakerEndpoint endpoint)
        {
            request.Headers.Add("Authorization", $"EndpointKey {endpoint.EndpointKey}");
            AddUserAgent(request);
        }

        private void AddUserAgent(HttpRequestMessage request)
        {
            // Bot Builder Package name and version
            var assemblyName = this.GetType().Assembly.GetName();
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(assemblyName.Name, assemblyName.Version.ToString()));

            // Platform information: OS and language runtime
            var framework = Assembly
                .GetEntryAssembly()?
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;
            var comment = $"({Environment.OSVersion.VersionString};{framework})";
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(comment));
        }
    }
}
