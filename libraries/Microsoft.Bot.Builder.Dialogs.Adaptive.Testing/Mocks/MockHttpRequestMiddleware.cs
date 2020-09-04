// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Mocks
{
    /// <summary>
    /// Middleware to mock http requests with an adapter.
    /// </summary>
    public class MockHttpRequestMiddleware : IMiddleware
    {
        private readonly HttpMessageHandler _httpMessageHandler;

        private readonly HttpClient _httpClient;

        private Func<HttpRequestMessage, HttpResponseMessage> _fallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockHttpRequestMiddleware"/> class.
        /// </summary>
        /// <param name="httpRequestMocks">mocks to use.</param>
        public MockHttpRequestMiddleware(List<HttpRequestMock> httpRequestMocks)
        {
            var handler = new MockHttpMessageHandler();
            foreach (var mock in httpRequestMocks)
            {
                mock.Setup(handler);
            }

            handler.Fallback.Respond((request) =>
            {
                if (_fallback != null)
                {
                    var result = _fallback(request);
                    if (result != null)
                    {
                        return result;
                    }
                }

                // Keep original behaviour
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = $"No matching mock handler for \"{request.Method} {request.RequestUri}\"",
                    Content = new StringContent(string.Empty),
                    RequestMessage = request,
                };
            });

            _httpMessageHandler = handler;
            var client = handler.ToHttpClient();
            _httpClient = client;
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            turnContext.TurnState.Add(_httpMessageHandler);
            turnContext.TurnState.Add(_httpClient);
            turnContext.TurnState.Add(this);
            await next(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Set fallback.
        /// </summary>
        /// <param name="fallback">New fallback or null.</param>
        public void SetFallback(Func<HttpRequestMessage, HttpResponseMessage> fallback)
        {
            _fallback = fallback;
        }
    }
}
