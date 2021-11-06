﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
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

        [Fact]
        public async void EmptyHeader_BotWithNoCredentials_ShouldThrow()
        {
            var header = string.Empty;
            var credentials = new SimpleCredentialProvider(string.Empty, string.Empty);

            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, null, emptyClient));
        }

        /// <summary>
        /// Tests with no authentication header and makes sure the service URL is not added to the trusted list.
        /// </summary>
        [Fact]
        public async void Channel_AuthenticationDisabled_ShouldBeAnonymous()
        {
            var header = string.Empty;
            var credentials = new SimpleCredentialProvider();

            var claimsPrincipal = await JwtTokenValidation.AuthenticateRequest(
                new Activity { ServiceUrl = "https://webchat.botframework.com/" },
                header,
                credentials,
                new SimpleChannelProvider(),
                emptyClient);

            Assert.Equal("anonymous", claimsPrincipal.AuthenticationType);
        }

        /// <summary>
        /// Tests with no authentication header and makes sure the service URL is not added to the trusted list.
        /// </summary>
        [Fact]
        public async void Channel_AuthenticationDisabled_ServiceUrlShouldNotBeTrusted()
        {
            var header = string.Empty;
            var credentials = new SimpleCredentialProvider();

            var claimsPrincipal = await JwtTokenValidation.AuthenticateRequest(
                new Activity { ServiceUrl = "https://webchat.botframework.com/" },
                header,
                credentials,
                new SimpleChannelProvider(),
                emptyClient);

            Assert.False(MicrosoftAppCredentials.IsTrustedServiceUrl("https://webchat.botframework.com/"));
        }

        [Fact]
        public async void GovernmentChannelValidation_Succeeds()
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
        public async void GovernmentChannelValidation_NoAuthentication_Fails()
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
        public async void GovernmentChannelValidation_NoAudienceClaim_Fails()
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
        public async void GovernmentChannelValidation_WrongAudienceClaim_Fails()
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
        public async void GovernmentChannelValidation_WrongAudienceClaimIssuer_Fails()
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
        public async void GovernmentChannelValidation_NoAudienceClaimValue_Fails()
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
        public async void GovernmentChannelValidation_NoServiceClaimValue_Fails()
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
        public async void GovernmentChannelValidation_WrongServiceClaimValue_Fails()
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
        public async void EnterpriseChannelValidation_Succeeds()
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
        public async void EnterpriseChannelValidation_NoAuthentication_Fails()
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
        public async void EnterpriseChannelValidation_NoAudienceClaim_Fails()
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
        public async void EnterpriseChannelValidation_WrongAudienceClaim_Fails()
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
        public async void EnterpriseChannelValidation_WrongAudienceClaimIssuer_Fails()
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
        public async void EnterpriseChannelValidation_NoAudienceClaimValue_Fails()
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
        public async void EnterpriseChannelValidation_NoServiceClaimValue_Fails()
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
        public async void EnterpriseChannelValidation_WrongServiceClaimValue_Fails()
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
