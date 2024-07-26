// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Moq;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class MsalServiceClientCredentialsFactoryTests
    {
        private const string TestAppId = nameof(TestAppId);
        private const string TestTenantId = nameof(TestTenantId);
        private const string TestAudience = nameof(TestAudience);
        private const string LoginEndpoint = "https://login.microsoftonline.com";
        private const string LoginEndpointGov = "https://login.microsoftonline.us/MicrosoftServices.onmicrosoft.us";
        private readonly Mock<ILogger> logger = new Mock<ILogger>();
        private readonly Mock<IConfiguration> configuration = new Mock<IConfiguration>();
        private readonly Mock<IConfidentialClientApplication> clientApplication = new Mock<IConfidentialClientApplication>();
        
        [Fact]
        public void ConstructorTests()
        {
            var factory = new MsalServiceClientCredentialsFactory(configuration.Object, clientApplication.Object, logger.Object);

            Assert.NotNull(factory);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldReturnEmptyCredentialsWithoutAppId(string appId)
        {
            var factory = new MsalServiceClientCredentialsFactory(configuration.Object, clientApplication.Object, logger.Object);
            var credentials = await factory.CreateCredentialsAsync(appId, TestAudience, LoginEndpoint, true, CancellationToken.None);

            Assert.Equal(MsalAppCredentials.Empty, credentials);
        }

        [Fact]
        public async Task ShouldThrowIfAppIdDoesNotMatch()
        {
            configuration.Setup(x => x.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey).Value).Returns(TestAppId);
            var factory = new MsalServiceClientCredentialsFactory(configuration.Object, clientApplication.Object, logger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => factory.CreateCredentialsAsync(
                    "InvalidAppId", TestAudience, LoginEndpoint, true, CancellationToken.None));
        }

        [Fact]
        public async Task ShouldCreateCredentials()
        {
            configuration.Setup(x => x.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey).Value).Returns(TestAppId);
            var factory = new MsalServiceClientCredentialsFactory(configuration.Object, clientApplication.Object, logger.Object);
            var credentials = await factory.CreateCredentialsAsync(TestAppId, TestAudience, LoginEndpoint, true, CancellationToken.None);

            Assert.NotNull(credentials);
            Assert.IsType<MsalAppCredentials>(credentials);
        }

        [Fact]
        public async Task ShouldCreateCredentialsForGoverment()
        {
            configuration.Setup(x => x.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey).Value).Returns(TestAppId);
            var factory = new MsalServiceClientCredentialsFactory(configuration.Object, clientApplication.Object, logger.Object);
            var credentials = await factory.CreateCredentialsAsync(TestAppId, TestAudience, LoginEndpointGov, true, CancellationToken.None);

            Assert.NotNull(credentials);
            Assert.IsType<MsalAppCredentials>(credentials);
        }

        [Fact]
        public async Task IsValidAppIdTest()
        {
            configuration.Setup(x => x.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey).Value).Returns(TestAppId);
            var factory = new MsalServiceClientCredentialsFactory(configuration.Object, clientApplication.Object, logger.Object);

            Assert.True(await factory.IsValidAppIdAsync(TestAppId, CancellationToken.None));
            Assert.False(await factory.IsValidAppIdAsync("InvalidAppId", CancellationToken.None));
        }

        [Fact]
        public async Task IsAuthenticationDisabledTest()
        {
            configuration.Setup(x => x.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey).Value).Returns(string.Empty);
            var factory = new MsalServiceClientCredentialsFactory(configuration.Object, clientApplication.Object, logger.Object);

            Assert.True(await factory.IsAuthenticationDisabledAsync(CancellationToken.None));
        }
    }
}
