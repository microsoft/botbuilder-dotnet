// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class CertificateServiceClientCredentialsFactoryTests
    {
        private const string TestAppId = nameof(TestAppId);
        private const string TestTenantId = nameof(TestTenantId);
        private const string TestAudience = nameof(TestAudience);
        private const string LoginEndpoint = AuthenticationConstants.ToChannelFromBotLoginUrlTemplate;
        private const string GovLoginEndpoint = GovernmentAuthenticationConstants.ToChannelFromBotLoginUrlTemplate;
        private const string PrivateLoginEndpoint = "https://login.privatecloud.com";
        private readonly Mock<ILogger> logger = new Mock<ILogger>();
        private readonly Mock<X509Certificate2> certificate = new Mock<X509Certificate2>();

        [Fact]
        public void ConstructorTests()
        {
            _ = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId);
            _ = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId, tenantId: TestTenantId);
            _ = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId, logger: logger.Object);
            _ = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId, httpClient: new HttpClient());
            _ = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId, httpClient: new HttpClient(), sendX5c: true);
        }

        [Fact]
        public void CannotCreateCredentialsFactoryWithoutCertificate()
        {
            Assert.Throws<ArgumentNullException>(() => new CertificateServiceClientCredentialsFactory(null, TestAppId));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void CannotCreateCredentialsFactoryWithoutAppId(string appId)
        {
            Assert.Throws<ArgumentNullException>(() => new CertificateServiceClientCredentialsFactory(certificate.Object, appId));
        }

        [Fact]
        public async Task IsValidAppIdTest()
        {
            var factory = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId);

            Assert.True(await factory.IsValidAppIdAsync(TestAppId, CancellationToken.None));
            Assert.False(await factory.IsValidAppIdAsync("InvalidAppId", CancellationToken.None));
        }

        [Fact]
        public async Task IsAuthenticationDisabledTest()
        {
            var factory = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId);

            Assert.False(await factory.IsAuthenticationDisabledAsync(CancellationToken.None));
        }

        [Fact]
        public async Task CanCreatePublicCredentials()
        {
            var factory = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId);

            var credentials = await factory.CreateCredentialsAsync(
                TestAppId, TestAudience, LoginEndpoint, true, CancellationToken.None);

            Assert.NotNull(credentials);
            Assert.IsType<CertificateAppCredentials>(credentials);
        }

        [Fact]
        public async Task CanCreateGovCredentials()
        {
            var factory = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId);

            var credentials = await factory.CreateCredentialsAsync(
                TestAppId, TestAudience, GovLoginEndpoint, true, CancellationToken.None);

            Assert.NotNull(credentials);
            Assert.IsType<CertificateGovernmentAppCredentials>(credentials);
        }

        [Fact]
        public async Task CanCreatePrivateCredentials()
        {
            var factory = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId);

            var credentials = await factory.CreateCredentialsAsync(
                TestAppId, TestAudience, PrivateLoginEndpoint, true, CancellationToken.None);

            Assert.NotNull(credentials);
            Assert.IsAssignableFrom<CertificateAppCredentials>(credentials);
            Assert.Equal(PrivateLoginEndpoint, ((CertificateAppCredentials)credentials).OAuthEndpoint);
        }

        [Fact]
        public async Task ShouldCreateUniqueCredentialsByAudience()
        {
            var factory = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId);

            var credentials1 = await factory.CreateCredentialsAsync(
                TestAppId, string.Empty, LoginEndpoint, true, CancellationToken.None);
            var credentials2 = await factory.CreateCredentialsAsync(
                TestAppId, TestAudience, LoginEndpoint, true, CancellationToken.None);
            var credentials3 = await factory.CreateCredentialsAsync(
                TestAppId, Guid.NewGuid().ToString(), LoginEndpoint, true, CancellationToken.None);
            var credentials4 = await factory.CreateCredentialsAsync(
                TestAppId, string.Empty, LoginEndpoint, true, CancellationToken.None);

            Assert.NotEqual(credentials1, credentials2);
            Assert.NotEqual(credentials1, credentials3);
            Assert.Equal(credentials1, credentials4);
        }

        [Fact]
        public async Task CannotCreateCredentialsWithInvalidAppId()
        {
            var factory = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId);

            await Assert.ThrowsAsync<InvalidOperationException>(() => factory.CreateCredentialsAsync(
                    "InvalidAppId", TestAudience, LoginEndpoint, true, CancellationToken.None));
        }
    }
}
