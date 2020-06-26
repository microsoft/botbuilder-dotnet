// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.Luis.TestUtils
{
    public class MockedHttpClientHandler : HttpClientHandler
    {
        private readonly HttpClient _client;

        public MockedHttpClientHandler(HttpClient client)
        {
            this._client = client;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var mockedRequest = new HttpRequestMessage()
            {
                RequestUri = request.RequestUri,
                Content = request.Content,
                Method = request.Method,
            };
            return _client.SendAsync(mockedRequest, cancellationToken);
        }
    }
}
