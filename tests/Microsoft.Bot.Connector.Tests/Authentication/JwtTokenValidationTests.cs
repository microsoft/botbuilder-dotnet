// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class JwtTokenValidationTests
    {
        private readonly HttpClient client;
        private readonly HttpClient emptyClient;

        public JwtTokenValidationTests()
        {
            // Disable TokenLifetime validation
            EmulatorValidation.ToBotFromEmulatorTokenValidationParameters.ValidateLifetime = false;
            ChannelValidation.ToBotFromChannelTokenValidationParameters.ValidateLifetime = false;
            client = new HttpClient
            {
                BaseAddress = new Uri("https://webchat.botframework.com/"),
            };
            emptyClient = new HttpClient();
        }

        // Disabled after appid was deleted. Issue created to move tests to functional tests
        //[Fact]
        //public async Task Connector_AuthHeader_CorrectAppIdAndServiceUrl_ShouldValidate()
        //{
        //    string header = $"Bearer {await new MicrosoftAppCredentials("", "").GetTokenAsync()}";
        //    var credentials = new SimpleCredentialProvider("", string.Empty);
        //    var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, "https://webchat.botframework.com/", client);

        //    Assert.True(result.IsAuthenticated);
        //}

        // Disabled after appid was deleted. Issue created to move tests to functional tests
        //[Fact]
        //public async Task Connector_AuthHeader_BotAppIdDiffers_ShouldNotValidate()
        //{
        //    string header = $"Bearer {await new MicrosoftAppCredentials("", "").GetTokenAsync()}";
        //    var credentials = new SimpleCredentialProvider("00000000-0000-0000-0000-000000000000", string.Empty);

        //    await Assert.ThrowsAsync<UnauthorizedAccessException>(
        //        async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, null, client));
        //}

        // Disabled after appid was deleted. Issue created to move tests to functional tests
        //[Fact]
        //public async Task Connector_AuthHeader_BotWithNoCredentials_ShouldNotValidate()
        //{
        //    // token received and auth disabled
        //    string header = $"Bearer {await new MicrosoftAppCredentials("", "").GetTokenAsync()}";
        //    var credentials = new SimpleCredentialProvider(string.Empty, string.Empty);

        //    await Assert.ThrowsAsync<UnauthorizedAccessException>(
        //        async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, null, client));
        //}

        [Fact]
        public async Task EmptyHeader_BotWithNoCredentials_ShouldThrow()
        {
            var header = string.Empty;
            var credentials = new SimpleCredentialProvider(string.Empty, string.Empty);

            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, null, emptyClient));
        }

        // Disabled after appid was deleted. Issue created to move tests to functional tests
        //[Fact]
        //public async Task Emulator_MsaHeader_CorrectAppIdAndServiceUrl_ShouldValidate()
        //{
        //    string header = $"Bearer {await new MicrosoftAppCredentials("", "").GetTokenAsync()}";
        //    var credentials = new SimpleCredentialProvider("", string.Empty);
        //    var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, "https://webchat.botframework.com/", emptyClient);

        //    Assert.True(result.IsAuthenticated);
        //}

        // Disabled after appid was deleted. Issue created to move tests to functional tests
        //[Fact]
        //public async Task Emulator_MsaHeader_BotAppIdDiffers_ShouldNotValidate()
        //{
        //    string header = $"Bearer {await new MicrosoftAppCredentials("", "").GetTokenAsync()}";
        //    var credentials = new SimpleCredentialProvider("00000000-0000-0000-0000-000000000000", string.Empty);
        //    await Assert.ThrowsAsync<UnauthorizedAccessException>(
        //        async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, null, emptyClient));
        //}

        /// <summary>
        /// Tests with no authentication header and makes sure the service URL is not added to the trusted list.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Channel_AuthenticationDisabled_ShouldBeAnonymous()
        {
            var header = string.Empty;
            var credentials = new SimpleCredentialProvider();

            var claimsPrincipal = await JwtTokenValidation.AuthenticateRequest(
                new Activity { ServiceUrl = "https://webchat.botframework.com/" },
                header,
                credentials,
                new SimpleChannelProvider(),
                emptyClient);

            Assert.Equal(AuthenticationConstants.AnonymousAuthType, claimsPrincipal.AuthenticationType);
        }

        /// <summary>
        /// Test with emulator channel Id and and RelatesTo set so it can validate we get an anonymous skill claim back.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Channel_AuthenticationDisabledAndSkill_ShouldBeAnonymous()
        {
            var header = string.Empty;
            var credentials = new SimpleCredentialProvider();

            var claimsPrincipal = await JwtTokenValidation.AuthenticateRequest(
                new Activity
                {
                    ChannelId = Channels.Emulator,
                    ServiceUrl = "https://webchat.botframework.com/",
                    RelatesTo = new ConversationReference(),
                    Recipient = new ChannelAccount { Role = RoleTypes.Skill }
                },
                header,
                credentials,
                new SimpleChannelProvider(),
                emptyClient);

            Assert.Equal(AuthenticationConstants.AnonymousAuthType, claimsPrincipal.AuthenticationType);
            Assert.Equal(AuthenticationConstants.AnonymousSkillAppId, JwtTokenValidation.GetAppIdFromClaims(claimsPrincipal.Claims));
        }

        // Disabled after appid was deleted. Issue created to move tests to functional tests
        //[Fact]
        //public async Task Emulator_AuthHeader_CorrectAppIdAndServiceUrl_WithGovChannelService_ShouldValidate()
        //{
        //    await JwtTokenValidation_ValidateAuthHeader_WithChannelService_Succeeds(
        //        "",         // emulator creds
        //        "",
        //        GovernmentAuthenticationConstants.ChannelService);
        //}

        // Disabled after appid was deleted. Issue created to move tests to functional tests
        //[Fact]
        //public async Task Emulator_AuthHeader_CorrectAppIdAndServiceUrl_WithPrivateChannelService_ShouldValidate()
        //{
        //    await JwtTokenValidation_ValidateAuthHeader_WithChannelService_Succeeds(
        //        "",         // emulator creds
        //        "",
        //        "TheChannel");
        //}

        // Disabled after appid was deleted. Issue created to move tests to functional tests
        //[Fact]
        //public async Task Connector_AuthHeader_CorrectAppIdAndServiceUrl_WithGovChannelService_ShouldValidate()
        //{
        //    await JwtTokenValidation_ValidateAuthHeader_WithChannelService_Succeeds(
        //        "",         // emulator creds
        //        "",
        //        GovernmentAuthenticationConstants.ChannelService);
        //}

        [Fact]
        public async Task GovernmentChannelValidation_Succeeds()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, appId, null, GovernmentAuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, true);
            await GovernmentChannelValidation.ValidateIdentity(identity, credentials, serviceUrl);
        }

        [Fact]
        public async Task GovernmentChannelValidation_NoAuthentication_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, appId, null, GovernmentAuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await GovernmentChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task GovernmentChannelValidation_NoAudienceClaim_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                  new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await GovernmentChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task GovernmentChannelValidation_WrongAudienceClaim_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, "abc", null, GovernmentAuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await GovernmentChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task GovernmentChannelValidation_WrongAudienceClaimIssuer_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, appId, null, "wrongissuer"),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await GovernmentChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task GovernmentChannelValidation_NoAudienceClaimValue_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, string.Empty, null, GovernmentAuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await GovernmentChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task GovernmentChannelValidation_NoServiceClaimValue_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, appId, null, GovernmentAuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, string.Empty, null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await GovernmentChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task GovernmentChannelValidation_WrongServiceClaimValue_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, appId, null, GovernmentAuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, "other", null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await GovernmentChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task EnterpriseChannelValidation_Succeeds()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, appId, null, AuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, true);
            await EnterpriseChannelValidation.ValidateIdentity(identity, credentials, serviceUrl);
        }

        [Fact]
        public async Task EnterpriseChannelValidation_NoAuthentication_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, appId, null, AuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await EnterpriseChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task EnterpriseChannelValidation_NoAudienceClaim_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await EnterpriseChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task EnterpriseChannelValidation_WrongAudienceClaim_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, "abc", null, AuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await EnterpriseChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task EnterpriseChannelValidation_WrongAudienceClaimIssuer_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, appId, null, "wrongissuer"),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await EnterpriseChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task EnterpriseChannelValidation_NoAudienceClaimValue_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, string.Empty, null, AuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl, null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await EnterpriseChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task EnterpriseChannelValidation_NoServiceClaimValue_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, appId, null, AuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, string.Empty, null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await EnterpriseChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public async Task EnterpriseChannelValidation_WrongServiceClaimValue_Fails()
        {
            var appId = "1234567890";
            var serviceUrl = "https://webchat.botframework.com/";
            var credentials = new SimpleCredentialProvider(appId, string.Empty);
            var identity = new SimpleClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(AuthenticationConstants.AudienceClaim, appId, null, AuthenticationConstants.ToBotFromChannelTokenIssuer),
                    new Claim(AuthenticationConstants.ServiceUrlClaim, "other", null),
                }, true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await EnterpriseChannelValidation.ValidateIdentity(identity, credentials, serviceUrl));
        }

        [Fact]
        public void GetAppIdFromClaimsTests()
        {
            var v1Claims = new List<Claim>();
            var v2Claims = new List<Claim>();
            var appId = Guid.NewGuid().ToString();

            // Empty list
            Assert.Null(JwtTokenValidation.GetAppIdFromClaims(v1Claims));

            // AppId there but no version (assumes v1)
            v1Claims.Add(new Claim(AuthenticationConstants.AppIdClaim, appId));
            Assert.Equal(appId, JwtTokenValidation.GetAppIdFromClaims(v1Claims));

            // AppId there with v1 version
            v1Claims.Add(new Claim(AuthenticationConstants.VersionClaim, "1.0"));
            Assert.Equal(appId, JwtTokenValidation.GetAppIdFromClaims(v1Claims));

            // v2 version but no azp
            v2Claims.Add(new Claim(AuthenticationConstants.VersionClaim, "2.0"));
            Assert.Null(JwtTokenValidation.GetAppIdFromClaims(v2Claims));

            // v2 version with azp
            v2Claims.Add(new Claim(AuthenticationConstants.AuthorizedParty, appId));
            Assert.Equal(appId, JwtTokenValidation.GetAppIdFromClaims(v2Claims));
        }

        [Fact]
        public async Task ValidateClaimsTest()
        {
            var claims = new List<Claim>();
            var defaultAuthConfig = new AuthenticationConfiguration();

            // No validator should pass.
            await JwtTokenValidation.ValidateClaimsAsync(defaultAuthConfig, claims);

            var mockValidator = new Mock<ClaimsValidator>();
            var authConfigWithClaimsValidator = new AuthenticationConfiguration() { ClaimsValidator = mockValidator.Object };

            // ClaimsValidator configured but no exception should pass.
            mockValidator.Setup(x => x.ValidateClaimsAsync(It.IsAny<List<Claim>>())).Returns(Task.CompletedTask);
            await JwtTokenValidation.ValidateClaimsAsync(authConfigWithClaimsValidator, claims);

            // Configure IClaimsValidator to fail
            mockValidator.Setup(x => x.ValidateClaimsAsync(It.IsAny<List<Claim>>())).Throws(new UnauthorizedAccessException("Invalid claims."));
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateClaimsAsync(authConfigWithClaimsValidator, claims));
            Assert.Equal("Invalid claims.", exception.Message);
        }

        [Fact]
        public async Task ValidateClaimsTest_ThrowsOnSkillClaim_WithNullValidator()
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(AuthenticationConstants.VersionClaim, "2.0"));
            claims.Add(new Claim(AuthenticationConstants.AudienceClaim, "SkillBotId"));
            claims.Add(new Claim(AuthenticationConstants.AuthorizedParty, "BotId")); // Skill claims aud!=azp

            // AuthenticationConfiguration with no ClaimsValidator and a Skill Claim, should throw UnauthorizedAccessException
            // Skill calls MUST be validated with a ClaimsValidator
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await JwtTokenValidation.ValidateClaimsAsync(new AuthenticationConfiguration(), claims));
        }

        [Fact]
        public async Task ValidateClaimsTest_DoesNotThrow_WhenNotSkillClaim_WithNullValidator()
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(AuthenticationConstants.VersionClaim, "2.0"));
            claims.Add(new Claim(AuthenticationConstants.AudienceClaim, "BotId"));
            claims.Add(new Claim(AuthenticationConstants.AuthorizedParty, "BotId")); // Skill claims aud!=azp

            // AuthenticationConfiguration with no ClaimsValidator and a none Skill Claim, should NOT throw UnauthorizedAccessException
            // None Skill do not need a ClaimsValidator.
            await JwtTokenValidation.ValidateClaimsAsync(new AuthenticationConfiguration(), claims);
        }

        [Fact]
        public void ValidationMetadataUrlTest_AseChannel_USGov()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMockChannelService = new Mock<IConfigurationSection>();
            configSectionMockChannelService.Setup(o => o.Value).Returns(GovernmentAuthenticationConstants.ChannelService);
            configMock.Setup(c => c.GetSection("ChannelService")).Returns(configSectionMockChannelService.Object);
            AseChannelValidation.Init(configMock.Object);

            Assert.Equal(GovernmentAuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl, typeof(AseChannelValidation).GetField("_metadataUrl", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(default));
        }

        [Fact]
        public void ValidationMetadataUrlTest_AseChannel_Public()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(o => o.Value).Returns(string.Empty);
            configMock.Setup(c => c.GetSection("ChannelService")).Returns(configSectionMock.Object);
            AseChannelValidation.Init(configMock.Object);

            Assert.Equal(AuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl, typeof(AseChannelValidation).GetField("_metadataUrl", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(default));
        }

        [Fact]
        public void ValidationIssueUrlTest_AseChannel()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(o => o.Value).Returns("testTenantId");
            configMock.Setup(c => c.GetSection("MicrosoftAppTenantId")).Returns(configSectionMock.Object);
            AseChannelValidation.Init(configMock.Object);

            var tenantIds = new string[]
            {
                "testTenantId",
                "f8cdef31-a31e-4b4a-93e4-5f571e91255a", // US Gov MicrosoftServices.onmicrosoft.us
                "d6d49420-f39b-4df7-a1dc-d59a935871db" // Public botframework.com
            };
            foreach (var tenantId in tenantIds)
            {
                Assert.Contains($"https://sts.windows.net/{tenantId}/", AseChannelValidation.BetweenBotAndAseChannelTokenValidationParameters.ValidIssuers);
                Assert.Contains($"https://login.microsoftonline.com/{tenantId}/v2.0", AseChannelValidation.BetweenBotAndAseChannelTokenValidationParameters.ValidIssuers);
                Assert.Contains($"https://login.microsoftonline.us/{tenantId}/v2.0", AseChannelValidation.BetweenBotAndAseChannelTokenValidationParameters.ValidIssuers);
            }
        }

        [Fact]
        public void ValidationChannelIdTest_AseChannel()
        {
            Assert.True(AseChannelValidation.IsAseChannel("AseChannel"));
        }

        private async Task JwtTokenValidation_ValidateAuthHeader_WithChannelService_Succeeds(string appId, string pwd, string channelService)
        {
            string header = $"Bearer {await new MicrosoftAppCredentials(appId, pwd).GetTokenAsync()}";
            await JwtTokenValidation_ValidateAuthHeader_WithChannelService_Succeeds(header, appId, pwd, channelService);
        }

        private async Task JwtTokenValidation_ValidateAuthHeader_WithChannelService_Succeeds(string header, string appId, string pwd, string channelService)
        {
            var credentials = new SimpleCredentialProvider(appId, pwd);
            var channel = new SimpleChannelProvider(channelService);
            var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, channel, string.Empty, "https://webchat.botframework.com/", client);

            Assert.True(result.IsAuthenticated);
        }

        private class SimpleClaimsIdentity : ClaimsIdentity
        {
            public SimpleClaimsIdentity(IEnumerable<Claim> claims, bool isAuthenticated)
                : base(claims)
            {
                IsAuthenticated = isAuthenticated;
            }

            public override bool IsAuthenticated { get; }
        }
    }
}
