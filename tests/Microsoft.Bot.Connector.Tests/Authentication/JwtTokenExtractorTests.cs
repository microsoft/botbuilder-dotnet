// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.IdentityModel.Protocols;
using Xunit;

namespace Microsoft.Bot.Connector.Authentication.Tests
{
    public class TestConfigurationRetriever : IConfigurationRetriever<IDictionary<string, HashSet<string>>>
    {
        public readonly Dictionary<string, HashSet<string>> EndorsementTable = new Dictionary<string, HashSet<string>>();

        public async Task<IDictionary<string, HashSet<string>>> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            return EndorsementTable;
        }
    }

    public class JwtTokenExtractorTests
    {
        private readonly HttpClient client;
        private readonly HttpClient emptyClient;

        private const string keyId = "CtfQC8Le-8NsC7oC2zQkZpcrfOc";
        private const string testChannelName = "testChannel";
        private const string complianceEndorsement = "o365Compliant";
        private const string randomEndorsement = "2112121212";

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
        public async Task Connector_TokenExtractor_NullRequiredEndorsements_ShouldValidate()
        {
            var configRetriever = new TestConfigurationRetriever();

            configRetriever.EndorsementTable.Add(keyId, new HashSet<string>() { randomEndorsement, complianceEndorsement, testChannelName});
            var claimsIdentity = await RunTestCase(configRetriever);
            Assert.True(claimsIdentity.IsAuthenticated);
        }

        [Fact]
        public async Task Connector_TokenExtractor_EmptyRequireEndorsements_ShouldValidate()
        {
            var configRetriever = new TestConfigurationRetriever();

            configRetriever.EndorsementTable.Add(keyId, new HashSet<string>() { randomEndorsement, complianceEndorsement, testChannelName });
            var claimsIdentity = await RunTestCase(configRetriever, new string[] { });
            Assert.True(claimsIdentity.IsAuthenticated);
        }

        [Fact]
        public async Task Connector_TokenExtractor_RequiredEndorsementsPresent_ShouldValidate()
        {
            var configRetriever = new TestConfigurationRetriever();

            configRetriever.EndorsementTable.Add(keyId, new HashSet<string>() { randomEndorsement, complianceEndorsement, testChannelName });
            var claimsIdentity = await RunTestCase(configRetriever, new string[] { complianceEndorsement });
            Assert.True(claimsIdentity.IsAuthenticated);
        }

        [Fact]
        public async Task Connector_TokenExtractor_RequiredEndorsementsPartiallyPresent_ShouldNotValidate()
        {
            var configRetriever = new TestConfigurationRetriever();

            configRetriever.EndorsementTable.Add(keyId, new HashSet<string>() { randomEndorsement, complianceEndorsement, testChannelName });
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await RunTestCase(configRetriever, new string[] { complianceEndorsement, "notSatisfiedEndorsement" }));
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
