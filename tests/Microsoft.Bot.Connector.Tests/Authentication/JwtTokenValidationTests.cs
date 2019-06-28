// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
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
        public async void Connector_AuthHeader_CorrectAppIdAndServiceUrl_ShouldValidate()
        {
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider("2cd87869-38a0-4182-9251-d056e8f0ac24", string.Empty);
            var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, "https://webchat.botframework.com/", client);

            Assert.True(result.IsAuthenticated);
        }

        [Fact]
        public async void Connector_AuthHeader_BotAppIdDiffers_ShouldNotValidate()
        {
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider("00000000-0000-0000-0000-000000000000", string.Empty);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, null, client));
        }

        [Fact]
        public async void Connector_AuthHeader_BotWithNoCredentials_ShouldNotValidate()
        {
            // token received and auth disabled
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider(string.Empty, string.Empty);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, null, client));
        }

        [Fact]
        public async void EmptyHeader_BotWithNoCredentials_ShouldThrow()
        {
            var header = string.Empty;
            var credentials = new SimpleCredentialProvider(string.Empty, string.Empty);

            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, null, emptyClient));
        }

        [Fact]
        public async void Emulator_MsaHeader_CorrectAppIdAndServiceUrl_ShouldValidate()
        {
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider("2cd87869-38a0-4182-9251-d056e8f0ac24", string.Empty);
            var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, "https://webchat.botframework.com/", emptyClient);

            Assert.True(result.IsAuthenticated);
        }

        [Fact]
        public async void Emulator_MsaHeader_BotAppIdDiffers_ShouldNotValidate()
        {
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider("00000000-0000-0000-0000-000000000000", string.Empty);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, new SimpleChannelProvider(), string.Empty, null, emptyClient));
        }

        /// <summary>
        /// Tests with a valid Token and service url; and ensures that Service url is added to Trusted service url list.
        /// </summary>
        [Fact]
        public async void Channel_MsaHeader_Valid_ServiceUrlShouldBeTrusted()
        {
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider("2cd87869-38a0-4182-9251-d056e8f0ac24", string.Empty);

            await JwtTokenValidation.AuthenticateRequest(
                new Activity { ServiceUrl = "https://smba.trafficmanager.net/amer-client-ss.msg/" },
                header,
                credentials,
                new SimpleChannelProvider(),
                emptyClient);

            Assert.True(MicrosoftAppCredentials.IsTrustedServiceUrl("https://smba.trafficmanager.net/amer-client-ss.msg/"));
        }

        /// <summary>
        /// Tests with a valid Token and invalid service url; and ensures that Service url is NOT added to Trusted service url list.
        /// </summary>
        [Fact]
        public async void Channel_MsaHeader_Invalid_ServiceUrlShouldNotBeTrusted()
        {
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider("7f74513e-6f96-4dbc-be9d-9a81fea22b88", string.Empty);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.AuthenticateRequest(
                new Activity { ServiceUrl = "https://webchat.botframework.com/" },
                header,
                credentials,
                new SimpleChannelProvider(),
                emptyClient));

            Assert.False(MicrosoftAppCredentials.IsTrustedServiceUrl("https://webchat.botframework.com/"));
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
        public async void Emulator_AuthHeader_CorrectAppIdAndServiceUrl_WithGovChannelService_ShouldValidate()
        {
            await JwtTokenValidation_ValidateAuthHeader_WithChannelService_Succeeds(
                "2cd87869-38a0-4182-9251-d056e8f0ac24",         // emulator creds
                "2.30Vs3VQLKt974F",
                GovernmentAuthenticationConstants.ChannelService);
        }

        [Fact]
        public async void Emulator_AuthHeader_CorrectAppIdAndServiceUrl_WithPrivateChannelService_ShouldValidate()
        {
            await JwtTokenValidation_ValidateAuthHeader_WithChannelService_Succeeds(
                "2cd87869-38a0-4182-9251-d056e8f0ac24",         // emulator creds
                "2.30Vs3VQLKt974F",
                "TheChannel");
        }

        [Fact]
        public async void Connector_AuthHeader_CorrectAppIdAndServiceUrl_WithGovChannelService_ShouldValidate()
        {
            await JwtTokenValidation_ValidateAuthHeader_WithChannelService_Succeeds(
                "2cd87869-38a0-4182-9251-d056e8f0ac24",         // emulator creds
                "2.30Vs3VQLKt974F",
                GovernmentAuthenticationConstants.ChannelService);
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

        private async Task JwtTokenValidation_ValidateAuthHeader_WithChannelService_Throws(string appId, string pwd, string channelService)
        {
            var header = $"Bearer {await new MicrosoftAppCredentials(appId, pwd).GetTokenAsync()}";
            await JwtTokenValidation_ValidateAuthHeader_WithChannelService_Succeeds(header, appId, pwd, channelService);
        }

        private async Task JwtTokenValidation_ValidateAuthHeader_WithChannelService_Throws(string header, string appId, string pwd, string channelService)
        {
            var credentials = new SimpleCredentialProvider(appId, pwd);
            var channel = new SimpleChannelProvider(channelService);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(
                    header,
                    credentials,
                    channel,
                    string.Empty,
                    "https://webchat.botframework.com/",
                    client));
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
