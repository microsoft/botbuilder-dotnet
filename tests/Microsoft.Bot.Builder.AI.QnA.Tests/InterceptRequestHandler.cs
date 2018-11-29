using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.QnA.Tests
{
    class InterceptRequestHandler : DelegatingHandler
    {
        public InterceptRequestHandler(HttpMessageHandler handler)
            : base(handler)
        {
        }

        public string Content { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Content = await ((StringContent)request.Content).ReadAsStringAsync();
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
