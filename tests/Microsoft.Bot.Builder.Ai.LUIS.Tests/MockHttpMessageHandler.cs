using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai.LUIS.Tests
{
    class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly IDictionary<Func<HttpRequestMessage, bool>, HttpResponseMessage> innerLinks;

        public MockHttpMessageHandler()
        {
            innerLinks = new Dictionary<Func<HttpRequestMessage, bool>, HttpResponseMessage>();
        }

        public void AddResponseForMatchRequest(Func<HttpRequestMessage, bool> requestMatch, HttpResponseMessage response)
        {
            innerLinks.Add(requestMatch, response);
        }

        public void AddResponseForRequestUri(Uri requestUri, HttpResponseMessage response)
        {
            innerLinks.Add(r => InnerMatch(HttpMethod.Get, requestUri, r), response);
        }

        public void AddResponseForRequestUri(Uri requestUri, string responseFile)
        {
            innerLinks.Add(r => InnerMatch(HttpMethod.Get, requestUri, r), ResponseFromFile(responseFile));
        }

        public void AddResponseForMethodAndRequestUri(HttpMethod method, Uri requestUri, HttpResponseMessage response)
        {
            innerLinks.Add(r => InnerMatch(method, requestUri, r), response);
        }

        private bool InnerMatch(HttpMethod method, Uri uri, HttpRequestMessage request)
        {
            return request.Method.Equals(method) && request.RequestUri.Equals(uri);
        }

        private HttpResponseMessage ResponseFromFile(string path)
        {
            var content = new StreamContent(File.OpenRead(path));
            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = content
            };
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(GetResponse(request));
        }

        private HttpResponseMessage GetResponse(HttpRequestMessage request)
        {
            foreach (var kv in innerLinks)
            {
                if (kv.Key(request))
                {
                    return kv.Value;
                }
            }
            throw new Exception("request without response");
        }
    }
}
