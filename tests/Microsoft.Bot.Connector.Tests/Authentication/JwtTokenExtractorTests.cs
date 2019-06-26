// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.IdentityModel.Protocols;
using Xunit;

namespace Microsoft.Bot.Connector.Authentication.Tests
{
    public class JwtTokenExtractorTests
    {
        private const string KeyId = "CtfQC8Le-8NsC7oC2zQkZpcrfOc";
        private const string TestChannelName = "testChannel";
        private const string ComplianceEndorsement = "o365Compliant";
        private const string RandomEndorsement = "2112121212";

        private readonly HttpClient client;
        private readonly HttpClient emptyClient;

        public JwtTokenExtractorTests()
        {
            ChannelValidation.ToBotFromChannelTokenValidationParameters.ValidateLifetime = false;
            ChannelValidation.ToBotFromChannelTokenValidationParameters.ValidateIssuer = false;

            client = new HttpClient
            {
                BaseAddress = new Uri("https://webchat.botframework.com/"),
            };
            emptyClient = new HttpClient();
        }

        [Fact]
        public async Task Connector_TokenExtractor_NullRequiredEndorsements_ShouldFail()
        {
            var configRetriever = new TestConfigurationRetriever();

            configRetriever.EndorsementTable.Add(KeyId, new HashSet<string>() { RandomEndorsement, ComplianceEndorsement, TestChannelName });
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await RunTestCase(configRetriever));
        }

        [Fact]
        public async Task Connector_TokenExtractor_EmptyRequireEndorsements_ShouldValidate()
        {
            var configRetriever = new TestConfigurationRetriever();

            configRetriever.EndorsementTable.Add(KeyId, new HashSet<string>() { RandomEndorsement, ComplianceEndorsement, TestChannelName });
            var claimsIdentity = await RunTestCase(configRetriever, new string[] { });
            Assert.True(claimsIdentity.IsAuthenticated);
        }

        [Fact]
        public async Task Connector_TokenExtractor_RequiredEndorsementsPresent_ShouldValidate()
        {
            var configRetriever = new TestConfigurationRetriever();

            configRetriever.EndorsementTable.Add(KeyId, new HashSet<string>() { RandomEndorsement, ComplianceEndorsement, TestChannelName });
            var claimsIdentity = await RunTestCase(configRetriever, new string[] { ComplianceEndorsement });
            Assert.True(claimsIdentity.IsAuthenticated);
        }

        [Fact]
        public async Task Connector_TokenExtractor_RequiredEndorsementsPartiallyPresent_ShouldNotValidate()
        {
            var configRetriever = new TestConfigurationRetriever();

            configRetriever.EndorsementTable.Add(KeyId, new HashSet<string>() { RandomEndorsement, ComplianceEndorsement, TestChannelName });
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await RunTestCase(configRetriever, new string[] { ComplianceEndorsement, "notSatisfiedEndorsement" }));
        }

        private async Task<ClaimsIdentity> RunTestCase(IConfigurationRetriever<IDictionary<string, HashSet<string>>> configRetriever, string[] requiredEndorsements = null)
        {
            var tokenExtractor = new JwtTokenExtractor(
                emptyClient,
                EmulatorValidation.ToBotFromEmulatorTokenValidationParameters,
                AuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl,
                AuthenticationConstants.AllowedSigningAlgorithms,
                new ConfigurationManager<IDictionary<string, HashSet<string>>>("http://test", configRetriever));

            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";

            return await tokenExtractor.GetIdentityAsync(header, "testChannel", requiredEndorsements);
        }
    }
}
