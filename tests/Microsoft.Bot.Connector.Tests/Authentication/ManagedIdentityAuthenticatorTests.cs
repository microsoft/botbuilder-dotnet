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
        public void CanGetJwtToken()
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

            var sut = new ManagedIdentityAuthenticator(appId(nameof(CanGetJwtToken)), audience(nameof(CanGetJwtToken)), httpClient);
            var token = sut.GetTokenAsync().GetAwaiter().GetResult();

            Assert.Equal("at_secret", token.AccessToken);
            Assert.Equal(expiresOn, token.ExpiresOn.ToUnixTimeSeconds());
        }

        [Theory]
        [InlineData(false, 1)]
        [InlineData(true, 2)]
        public void CanGetJwtTokenWithForceRefresh(bool forceRefreshInput, int index)
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
            var token = sut.GetTokenAsync(forceRefreshInput).GetAwaiter().GetResult();

            Assert.Equal("at_secret", token.AccessToken);
            Assert.Equal(expiresOn, token.ExpiresOn.ToUnixTimeSeconds());
        }

        [Fact]
        public void DefaultRetryOnException()
        {
            var maxRetries = 10;
            var callsToAcquireToken = 0;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => 
                {
                    callsToAcquireToken++;
                    throw new Exception();
                });
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var sut = new ManagedIdentityAuthenticator(appId(nameof(DefaultRetryOnException)), audience(nameof(DefaultRetryOnException)), httpClient);

            try
            {
                _ = sut.GetTokenAsync().GetAwaiter().GetResult();
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
        public void CanRetryAndAcquireToken()
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
                        throw new Exception();
                    }

                    return response;
                });
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var sut = new ManagedIdentityAuthenticator(appId(nameof(CanRetryAndAcquireToken)), audience(nameof(CanRetryAndAcquireToken)), httpClient);
            var token = sut.GetTokenAsync().GetAwaiter().GetResult();

            Assert.Equal("at_secret", token.AccessToken);
            Assert.Equal(expiresOn, token.ExpiresOn.ToUnixTimeSeconds());
            Assert.Equal(2, callsToAcquireToken);
        }
    }
}
