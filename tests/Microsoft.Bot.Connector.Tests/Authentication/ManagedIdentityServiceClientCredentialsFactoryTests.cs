// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class ManagedIdentityServiceClientCredentialsFactoryTests
    {
        private const string TestAppId = "foo";
        private const string TestAudience = "bar";

        [Fact]
        public void ConstructorTests()
        {
            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();

            _ = new ManagedIdentityServiceClientCredentialsFactory(TestAppId, tokenProviderFactory.Object);

            using (var customHttpClient = new HttpClient())
            {
                _ = new ManagedIdentityServiceClientCredentialsFactory(TestAppId, tokenProviderFactory.Object, customHttpClient);

                var logger = new Mock<ILogger>();
                _ = new ManagedIdentityServiceClientCredentialsFactory(TestAppId, tokenProviderFactory.Object, null, logger.Object);

                _ = new ManagedIdentityServiceClientCredentialsFactory(TestAppId, tokenProviderFactory.Object, customHttpClient, logger.Object);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void CannotCreateCredentialsFactoryWithoutAppId(string appId)
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();
                _ = new ManagedIdentityServiceClientCredentialsFactory(appId, tokenProviderFactory.Object);
            });
        }

        [Fact]
        public void CanCreateCredentialsFactoryWithoutTokenProviderFactory()
        {
            _ = new ManagedIdentityServiceClientCredentialsFactory(TestAppId, tokenProviderFactory: null);
        }

        [Fact]
        public void IsValidAppIdTest()
        {
            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();
            var sut = new ManagedIdentityServiceClientCredentialsFactory(TestAppId, tokenProviderFactory.Object);

            Assert.True(sut.IsValidAppIdAsync(TestAppId, CancellationToken.None).GetAwaiter().GetResult());
            Assert.False(sut.IsValidAppIdAsync("InvalidAppId", CancellationToken.None).GetAwaiter().GetResult());
        }

        [Fact]
        public void IsAuthenticationDisabledTest()
        {
            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();
            var sut = new ManagedIdentityServiceClientCredentialsFactory(TestAppId, tokenProviderFactory.Object);

            Assert.False(sut.IsAuthenticationDisabledAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Fact]
        public void CanCreateCredentials()
        {
            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();
            var sut = new ManagedIdentityServiceClientCredentialsFactory(TestAppId, tokenProviderFactory.Object);

            var credentials = sut.CreateCredentialsAsync(
                TestAppId, TestAudience, "https://login.microsoftonline.com", true, CancellationToken.None);
            Assert.NotNull(credentials);
        }

        [Fact]
        public void CannotCreateCredentialsWithInvalidAppId()
        {
            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();
            var sut = new ManagedIdentityServiceClientCredentialsFactory(TestAppId, tokenProviderFactory.Object);

            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = sut.CreateCredentialsAsync(
                    "InvalidAppId", TestAudience, "https://login.microsoftonline.com", true, CancellationToken.None);
            });
        }
    }
}
