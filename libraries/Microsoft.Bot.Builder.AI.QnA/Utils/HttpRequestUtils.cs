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
    internal class HttpRequestUtils
    {
        private static ProductInfoHeaderValue botBuilderInfo;
        private static ProductInfoHeaderValue platformInfo;

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestUtils"/> class.
        /// </summary>
        /// <param name="httpClient">Http client.</param>
        public HttpRequestUtils(HttpClient httpClient)
        {
            this._httpClient = httpClient;
            this.UpdateBotBuilderAndPlatformInfo();
        }

        /// <summary>
        /// Execute Http request.
        /// </summary>
        /// <param name="requestUrl">Http request url.</param>
        /// <param name="payloadBody">Http request body.</param>
        /// <param name="endpoint">QnA Maker endpoint details.</param>
        /// <returns>Returns http response object.</returns>
        public async Task<HttpResponseMessage> ExecuteHttpRequestAsync(string requestUrl, string payloadBody, QnAMakerEndpoint endpoint)
        {
            if (requestUrl == null)
            {
                throw new ArgumentNullException(nameof(requestUrl), "Request url can not be null.");
            }

            if (payloadBody == null)
            {
                throw new ArgumentNullException(nameof(payloadBody), "Payload body can not be null.");
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

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
            request.Headers.Add("Ocp-Apim-Subscription-Key", endpoint.EndpointKey); 
            request.Headers.UserAgent.Add(botBuilderInfo);
            request.Headers.UserAgent.Add(platformInfo);
        }

        private void UpdateBotBuilderAndPlatformInfo()
        {
            // Bot Builder Package name and version
            var assemblyName = this.GetType().Assembly.GetName();
            botBuilderInfo = new ProductInfoHeaderValue(assemblyName.Name, assemblyName.Version.ToString());

            // Platform information: OS and language runtime
            var framework = Assembly
                .GetEntryAssembly()?
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;
            var comment = $"({Environment.OSVersion.VersionString};{framework})";
            platformInfo = new ProductInfoHeaderValue(comment);
        }
    }
}
