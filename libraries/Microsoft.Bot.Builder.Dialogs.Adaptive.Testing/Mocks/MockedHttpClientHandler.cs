// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.Luis.Testing
{
    public class MockedHttpClientHandler : HttpClientHandler
    {
        private readonly HttpClient client;

        public MockedHttpClientHandler(HttpClient client)
        {
            this.client = client;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var mockedRequest = new HttpRequestMessage()
            {
                RequestUri = request.RequestUri,
                Content = request.Content,
                Method = request.Method,
            };
#pragma warning restore CA2000 // Dispose objects before losing scope
            return client.SendAsync(mockedRequest, cancellationToken);
        }
    }
}
