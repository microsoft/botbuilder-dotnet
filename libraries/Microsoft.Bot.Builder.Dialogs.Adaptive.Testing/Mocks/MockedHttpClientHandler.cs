// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.Luis.Testing
{
    /// <summary>
    /// HttpChandler mock.
    /// </summary>
    public class MockedHttpClientHandler : HttpClientHandler
    {
        private readonly HttpClient _client;
        private readonly HttpMessageHandler _httpMessageHandler;
        private readonly MethodInfo httpMessageHandlerMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockedHttpClientHandler"/> class.
        /// </summary>
        /// <param name="client">client to use.</param>
        public MockedHttpClientHandler(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockedHttpClientHandler"/> class.
        /// </summary>
        /// <param name="httpMessageHandler">Handler to use.</param>
        public MockedHttpClientHandler(HttpMessageHandler httpMessageHandler)
        {
            _httpMessageHandler = httpMessageHandler;

            // Call directly to avoid wrapping with HttpClient.
            httpMessageHandlerMethod = httpMessageHandler.GetType().GetMethod(
                    nameof(SendAsync),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    new[] { typeof(HttpRequestMessage), typeof(CancellationToken) },
                    null);
        }

        /// <summary>
        /// Clone everything of a HttpRequestMessage.
        /// </summary>
        /// <param name="request">The HttpRequestMessage to clone.</param>
        /// <returns>The cloned HttpRequestMessage.</returns>
        public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            HttpRequestMessage clone = new HttpRequestMessage(request.Method, request.RequestUri);
#pragma warning disable CA2000 // Dispose objects before losing scope
            var memoryStream = new MemoryStream();
#pragma warning restore CA2000 // Dispose objects before losing scope
            if (request.Content != null)
            {
                await request.Content.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStream.Position = 0;
                clone.Content = new StreamContent(memoryStream);

                // Copy the content headers
                if (request.Content.Headers != null)
                {
                    foreach (var h in request.Content.Headers)
                    {
                        clone.Content.Headers.Add(h.Key, h.Value);
                    }
                }
            }

            clone.Version = request.Version;

            foreach (KeyValuePair<string, object> prop in request.Properties)
            {
                clone.Properties.Add(prop);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        /// <inheritdoc/>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_httpMessageHandler != null)
            {
                return (Task<HttpResponseMessage>)httpMessageHandlerMethod.Invoke(_httpMessageHandler, new object[] { request, cancellationToken });
            }

#pragma warning disable CA2000 // Dispose objects before losing scope
            var mockedRequest = new HttpRequestMessage()
            {
                RequestUri = request.RequestUri,
                Content = request.Content,
                Method = request.Method,
            };
#pragma warning restore CA2000 // Dispose objects before losing scope
            return _client.SendAsync(mockedRequest, cancellationToken);
        }
    }
}
