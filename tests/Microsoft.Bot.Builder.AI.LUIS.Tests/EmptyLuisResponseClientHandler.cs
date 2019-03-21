// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.Luis.Tests
{
    /// <summary>
    /// This HttpClientHandler returns a hard coded response equivallent to a LUIS no-match-found result.
    /// </summary>
    public class EmptyLuisResponseClientHandler : HttpClientHandler
    {
        public string UserAgent { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Grab the user-agent so we can examine it after the call completes.
            UserAgent = request.Headers.UserAgent.ToString();

            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ \"query\": null, \"intents\": [], \"entities\": [] }"),
            });
        }
    }
}
