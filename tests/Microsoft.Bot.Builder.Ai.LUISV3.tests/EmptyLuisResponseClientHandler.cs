// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.LuisV3.Tests
{
    /// <inheritdoc />
    /// <summary>
    /// An HttpClientHandler that returns a hard coded response equivalent to a LUIS no-match-found result.
    /// </summary>
    public class EmptyLuisResponseClientHandler : HttpClientHandler
    {
        public string UserAgent { get; private set; }

        public HttpRequestMessage RequestMessage { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Capture the user-agent and the HttpRequestMessage so we can examine it after the call completes.
            UserAgent = request.Headers.UserAgent.ToString();
            RequestMessage = request;

            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ \"query\": null, \"prediction\": { \"intents\": {}, \"entities\": {} }}"),
            });
        }
    }
}
