// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.QnA.Tests
{
    public class InterceptRequestHandler : DelegatingHandler
    {
        public InterceptRequestHandler(HttpMessageHandler handler)
            : base(handler)
        {
        }

        public string UserAgent { get; private set; }

        public string Content { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Grab the user-agent so we can examine it after the call completes.
            UserAgent = request.Headers.UserAgent.ToString();

            // Grab the content we are sending so we can examine it after the call completes.
            Content = await ((StringContent)request.Content).ReadAsStringAsync();

            // Forward the call.
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
