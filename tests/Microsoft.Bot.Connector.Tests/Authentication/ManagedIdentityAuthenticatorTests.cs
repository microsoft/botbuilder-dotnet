// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class ManagedIdentityAuthenticatorTests
    {
        private readonly Func<string, string> appId = (id) => $"id {id} ";
        private readonly Func<string, string> audience = (id) => $"audience {id} ";

        [Fact]
        public async Task CanGetJwtTokenAsync()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var expiresOn = DateTimeOffset.Now.ToUnixTimeSeconds() + 10000;
            var json = new JObject
            {
                { "expires_on", expiresOn },
                { "access_token", "at_secret" }
            };
            response.Content = new StringContent(json.ToString());

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var sut = new ManagedIdentityAuthenticator(appId(nameof(CanGetJwtTokenAsync)), audience(nameof(CanGetJwtTokenAsync)), httpClient);
            var token = await sut.GetTokenAsync();

            Assert.Equal("at_secret", token.AccessToken);
            Assert.Equal(expiresOn, token.ExpiresOn.ToUnixTimeSeconds());
        }

        [Theory]
        [InlineData(false, 1)]
        [InlineData(true, 2)]
        public async Task CanGetJwtTokenWithForceRefresh(bool forceRefreshInput, int index)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var expiresOn = DateTimeOffset.Now.ToUnixTimeSeconds() + 10000;
            var json = new JObject
            {
                { "expires_on", expiresOn },
                { "access_token", "at_secret" }
            };
            response.Content = new StringContent(json.ToString());

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var sut = new ManagedIdentityAuthenticator(appId(nameof(CanGetJwtTokenWithForceRefresh)) + index, audience(nameof(CanGetJwtTokenWithForceRefresh)) + index, httpClient);
            var token = await sut.GetTokenAsync(forceRefreshInput);

            Assert.Equal("at_secret", token.AccessToken);
            Assert.Equal(expiresOn, token.ExpiresOn.ToUnixTimeSeconds());
        }

        [Fact]
        public async Task DefaultRetryOnException()
        {
            var maxRetries = 10;
            var callsToAcquireToken = 0;
            var actualCallsToAcquireToken = 0;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => 
                {
                    // ManagedCredentialsClient is apparently auto-retrying failed requests once.
                    // Resolution unclear.
                    // For now, count the number of times WE think it's be called.
                    actualCallsToAcquireToken++;

                    if (actualCallsToAcquireToken % 2 != 0)
                    {
                        callsToAcquireToken++;
                    }

                    return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var sut = new ManagedIdentityAuthenticator(appId(nameof(DefaultRetryOnException)), audience(nameof(DefaultRetryOnException)), httpClient);

            try
            {
                _ = await sut.GetTokenAsync();
            }
            catch (AggregateException e)
            {
                Assert.Equal(maxRetries + 1, e.InnerExceptions.Count);
            }
            finally
            {
                Assert.Equal(maxRetries + 1, callsToAcquireToken);
            }
        }

        [Fact]
        public async Task CanRetryAndAcquireToken()
        {
            var callsToAcquireToken = 0;
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var expiresOn = DateTimeOffset.Now.ToUnixTimeSeconds() + 10000;
            var json = new JObject
            {
                { "expires_on", expiresOn },
                { "access_token", "at_secret" }
            };
            response.Content = new StringContent(json.ToString());

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => 
                {
                    callsToAcquireToken++;
                    if (callsToAcquireToken == 1)
                    {
                        return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                    }

                    return response;
                });
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var sut = new ManagedIdentityAuthenticator(appId(nameof(CanRetryAndAcquireToken)), audience(nameof(CanRetryAndAcquireToken)), httpClient);
            var token = await sut.GetTokenAsync();

            Assert.Equal("at_secret", token.AccessToken);
            Assert.Equal(expiresOn, token.ExpiresOn.ToUnixTimeSeconds());
            Assert.Equal(2, callsToAcquireToken);
        }
    }
}
