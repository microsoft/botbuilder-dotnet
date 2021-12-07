// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.FunctionalTests.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.FunctionalTests
{
    [TestClass]
    [TestCategory("FunctionalTests")]
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
            client = new HttpClient
            {
                BaseAddress = new Uri("https://webchat.botframework.com/"),
            };
            emptyClient = new HttpClient();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Connector_TokenExtractor_NullRequiredEndorsements_ShouldFail()
        {
            var configRetriever = new TestConfigurationRetriever();

            configRetriever.EndorsementTable.Add(KeyId, new HashSet<string>() { RandomEndorsement, ComplianceEndorsement, TestChannelName });
            await RunTestCase(configRetriever);
        }

        [TestMethod]
        public async Task Connector_TokenExtractor_EmptyRequireEndorsements_ShouldValidate()
        {
            var configRetriever = new TestConfigurationRetriever();

            configRetriever.EndorsementTable.Add(KeyId, new HashSet<string>() { RandomEndorsement, ComplianceEndorsement, TestChannelName });
            var claimsIdentity = await RunTestCase(configRetriever, new string[] { });
            Assert.IsTrue(claimsIdentity.IsAuthenticated);
        }

        [TestMethod]
        public async Task Connector_TokenExtractor_RequiredEndorsementsPresent_ShouldValidate()
        {
            var configRetriever = new TestConfigurationRetriever();

            configRetriever.EndorsementTable.Add(KeyId, new HashSet<string>() { RandomEndorsement, ComplianceEndorsement, TestChannelName });
            var claimsIdentity = await RunTestCase(configRetriever, new string[] { ComplianceEndorsement });
            Assert.IsTrue(claimsIdentity.IsAuthenticated);
        }

        private async Task<ClaimsIdentity> RunTestCase(IConfigurationRetriever<IDictionary<string, HashSet<string>>> configRetriever, string[] requiredEndorsements = null)
        {
            var emulatorTokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    "https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/", // Auth v3.1, 1.0 token
                    "https://login.microsoftonline.com/d6d49420-f39b-4df7-a1dc-d59a935871db/v2.0", // Auth v3.1, 2.0 token
                    "https://sts.windows.net/f8cdef31-a31e-4b4a-93e4-5f571e91255a/", // Auth v3.2, 1.0 token
                    "https://login.microsoftonline.com/f8cdef31-a31e-4b4a-93e4-5f571e91255a/v2.0", // Auth v3.2, 2.0 token
                    "https://sts.windows.net/cab8a31a-1906-4287-a0d8-4eef66b95f6e/", // Auth for US Gov, 1.0 token
                    "https://login.microsoftonline.us/cab8a31a-1906-4287-a0d8-4eef66b95f6e/v2.0", // Auth for US Gov, 2.0 token
                },
                ValidateAudience = false, // Audience validation takes place manually in code.
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
            };

            var tokenExtractor = new JwtTokenExtractor(
                emptyClient,
                emulatorTokenValidationParameters,
                AuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl,
                AuthenticationConstants.AllowedSigningAlgorithms,
                new ConfigurationManager<IDictionary<string, HashSet<string>>>("http://test", configRetriever));

            string header = $"Bearer {await new MicrosoftAppCredentials(EnvironmentConfig.TestAppId(), EnvironmentConfig.TestAppPassword()).GetTokenAsync()}";

            return await tokenExtractor.GetIdentityAsync(header, "testChannel", requiredEndorsements);
        }
    }
}
