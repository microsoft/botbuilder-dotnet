// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
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
        private readonly Mock<ILogger> logger = new Mock<ILogger>();
        private readonly Mock<X509Certificate2> certificate = new Mock<X509Certificate2>();

        [Fact]
        public void ConstructorTests()
        {
            _ = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId);
            _ = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId, tenantId: TestTenantId);
            _ = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId, logger: logger.Object);
            _ = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId, httpClient: new HttpClient());
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
        public void IsValidAppIdTest()
        {
            var factory = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId);

            Assert.True(factory.IsValidAppIdAsync(TestAppId, CancellationToken.None).GetAwaiter().GetResult());
            Assert.False(factory.IsValidAppIdAsync("InvalidAppId", CancellationToken.None).GetAwaiter().GetResult());
        }

        [Fact]
        public void IsAuthenticationDisabledTest()
        {
            var factory = new CertificateServiceClientCredentialsFactory(certificate.Object, TestAppId);

            Assert.False(factory.IsAuthenticationDisabledAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Fact]
        public async void CanCreateCredentials()
        {
            var factory = new CertificateServiceClientCredentialsFactory(certificate.Object, false, TestAppId);

            var credentials = await factory.CreateCredentialsAsync(
                TestAppId, TestAudience, "https://login.microsoftonline.com", true, CancellationToken.None);

            Assert.NotNull(credentials);
            Assert.IsType<CertificateAppCredentials>(credentials);
        }

        [Fact]
        public void CannotCreateCredentialsWithInvalidAppId()
        {
            var factory = new CertificateServiceClientCredentialsFactory(certificate.Object, false, TestAppId);

            Assert.ThrowsAsync<InvalidOperationException>(() => factory.CreateCredentialsAsync(
                    "InvalidAppId", TestAudience, "https://login.microsoftonline.com", true, CancellationToken.None));
        }
    }
}
