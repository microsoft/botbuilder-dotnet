// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class ManagedIdentityAuthenticatorTests
    {
        private const string TestAppId = "foo";
        private const string TestAudience = "bar";
        private const string TestConnectionString = "RunAs=App;AppId=foo";
        private const string TestAzureAdInstance = "https://login.microsoftonline.com/";

        [Fact]
        public void ConstructorTests()
        {
            var callsToCreateTokenProvider = 0;

            var tokenProvider = new Mock<AzureServiceTokenProvider>(TestConnectionString, TestAzureAdInstance);

            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();
            tokenProviderFactory
                .Setup(f => f.CreateAzureServiceTokenProvider(It.IsAny<string>(), It.IsAny<HttpClient>()))
                .Returns<string, HttpClient>((appId, customHttpClient) =>
                {
                    callsToCreateTokenProvider++;
                    Assert.Equal(TestAppId, appId);

                    return tokenProvider.Object;
                });

            _ = new ManagedIdentityAuthenticator(TestAppId, TestAudience, tokenProviderFactory.Object);

            using (var customHttpClient = new HttpClient())
            {
                _ = new ManagedIdentityAuthenticator(TestAppId, TestAudience, tokenProviderFactory.Object, customHttpClient);

                var logger = new Mock<ILogger>();
                _ = new ManagedIdentityAuthenticator(TestAppId, TestAudience, tokenProviderFactory.Object, null, logger.Object);

                _ = new ManagedIdentityAuthenticator(TestAppId, TestAudience, tokenProviderFactory.Object, customHttpClient, logger.Object);
            }

            Assert.Equal(4, callsToCreateTokenProvider);
        }

        [Fact]
        public void CanGetJwtToken()
        {
            var authResult = new AppAuthenticationResult();
            var tokenProvider = new Mock<AzureServiceTokenProvider>(TestConnectionString, TestAzureAdInstance);
            tokenProvider
                .Setup(p => p.GetAuthenticationResultAsync(
                    It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns<string, bool, CancellationToken>((resource, forceRefresh, cancellationToken) =>
                {
                    Assert.False(forceRefresh);
                    return Task.FromResult(authResult);
                });

            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();
            tokenProviderFactory
                .Setup(f => f.CreateAzureServiceTokenProvider(It.IsAny<string>(), It.IsAny<HttpClient>()))
                .Returns<string, HttpClient>((appId, customHttpClient) => tokenProvider.Object);

            var sut = new ManagedIdentityAuthenticator(TestAppId, TestAudience, tokenProviderFactory.Object);
            var token = sut.GetTokenAsync().GetAwaiter().GetResult();

            Assert.Equal(authResult.AccessToken, token.AccessToken);
            Assert.Equal(authResult.ExpiresOn, token.ExpiresOn);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CanGetJwtTokenWithForceRefresh(bool forceRefreshInput)
        {
            var authResult = new AppAuthenticationResult();
            var tokenProvider = new Mock<AzureServiceTokenProvider>(TestConnectionString, TestAzureAdInstance);
            tokenProvider
                .Setup(p => p.GetAuthenticationResultAsync(
                    It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns<string, bool, CancellationToken>((resource, forceRefresh, cancellationToken) =>
                {
                    Assert.Equal(forceRefreshInput, forceRefresh);
                    return Task.FromResult(authResult);
                });

            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();
            tokenProviderFactory
                .Setup(f => f.CreateAzureServiceTokenProvider(It.IsAny<string>(), It.IsAny<HttpClient>()))
                .Returns<string, HttpClient>((appId, customHttpClient) => tokenProvider.Object);

            var sut = new ManagedIdentityAuthenticator(TestAppId, TestAudience, tokenProviderFactory.Object);
            var token = sut.GetTokenAsync(forceRefreshInput).GetAwaiter().GetResult();

            Assert.Equal(authResult.AccessToken, token.AccessToken);
            Assert.Equal(authResult.ExpiresOn, token.ExpiresOn);
        }

        [Fact]
        public void DefaultRetryOnException()
        {
            var maxRetries = 10;
            var callsToAcquireToken = 0;

            var tokenProvider = new Mock<AzureServiceTokenProvider>(TestConnectionString, TestAzureAdInstance);
            tokenProvider
                .Setup(p => p.GetAuthenticationResultAsync(
                    It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns<string, bool, CancellationToken>((resource, forceRefresh, cancellationToken) =>
                {
                    callsToAcquireToken++;
                    throw new Exception();
                });

            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();
            tokenProviderFactory
                .Setup(f => f.CreateAzureServiceTokenProvider(It.IsAny<string>(), It.IsAny<HttpClient>()))
                .Returns<string, HttpClient>((appId, customHttpClient) => tokenProvider.Object);

            var sut = new ManagedIdentityAuthenticator(TestAppId, TestAudience, tokenProviderFactory.Object);

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

            var authResult = new AppAuthenticationResult();
            var tokenProvider = new Mock<AzureServiceTokenProvider>(TestConnectionString, TestAzureAdInstance);
            tokenProvider
                .Setup(p => p.GetAuthenticationResultAsync(
                    It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns<string, bool, CancellationToken>((resource, forceRefresh, cancellationToken) =>
                {
                    callsToAcquireToken++;
                    if (callsToAcquireToken == 1)
                    {
                        throw new Exception();
                    }

                    return Task.FromResult(authResult);
                });

            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();
            tokenProviderFactory
                .Setup(f => f.CreateAzureServiceTokenProvider(It.IsAny<string>(), It.IsAny<HttpClient>()))
                .Returns<string, HttpClient>((appId, customHttpClient) => tokenProvider.Object);

            var sut = new ManagedIdentityAuthenticator(TestAppId, TestAudience, tokenProviderFactory.Object);
            var token = sut.GetTokenAsync().GetAwaiter().GetResult();

            Assert.Equal(authResult.AccessToken, token.AccessToken);
            Assert.Equal(authResult.ExpiresOn, token.ExpiresOn);

            Assert.Equal(2, callsToAcquireToken);
        }
    }
}
