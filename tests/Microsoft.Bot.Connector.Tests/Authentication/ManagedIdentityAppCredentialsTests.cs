// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class ManagedIdentityAppCredentialsTests
    {
        private const string TestAppId = "foo";
        private const string TestAudience = "bar";

        [Fact]
        public void ConstructorTests()
        {
            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();

            var sut1 = new ManagedIdentityAppCredentials(TestAppId, TestAudience, tokenProviderFactory.Object);
            Assert.Equal(TestAppId, sut1.MicrosoftAppId);
            Assert.Equal(TestAudience, sut1.OAuthScope);

            using (var customHttpClient = new HttpClient())
            {
                var sut2 = new ManagedIdentityAppCredentials(TestAppId, TestAudience, tokenProviderFactory.Object, customHttpClient);
                Assert.Equal(TestAppId, sut2.MicrosoftAppId);
                Assert.Equal(TestAudience, sut2.OAuthScope);

                var logger = new Mock<ILogger>().Object;
                var sut3 = new ManagedIdentityAppCredentials(TestAppId, TestAudience, tokenProviderFactory.Object, null, logger);
                Assert.Equal(TestAppId, sut3.MicrosoftAppId);
                Assert.Equal(TestAudience, sut3.OAuthScope);

                var sut4 = new ManagedIdentityAppCredentials(TestAppId, TestAudience, tokenProviderFactory.Object, customHttpClient, logger);
                Assert.Equal(TestAppId, sut4.MicrosoftAppId);
                Assert.Equal(TestAudience, sut4.OAuthScope);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void CanCreateCredentialsWithoutAudience(string audience)
        {
            var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();

            var sut = new ManagedIdentityAppCredentials(TestAppId, audience, tokenProviderFactory.Object);
            Assert.Equal(TestAppId, sut.MicrosoftAppId);
            Assert.Equal(AuthenticationConstants.ToChannelFromBotOAuthScope, sut.OAuthScope);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void CannotCreateCredentialsWithoutAppId(string appId)
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var tokenProviderFactory = new Mock<IJwtTokenProviderFactory>();
                _ = new ManagedIdentityAppCredentials(appId, TestAudience, tokenProviderFactory.Object);
            });
        }

        [Fact]
        public void CanCreateCredentialsWithoutTokenProviderFactory()
        {
            _ = new ManagedIdentityAppCredentials(TestAppId, TestAudience, tokenProviderFactory: null);
        }
    }
}
