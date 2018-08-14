using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Connector.Tests
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
                BaseAddress = new Uri("https://webchat.botframework.com/")
            };
            emptyClient = new HttpClient();
        }

        [Fact]
        public async void Connector_AuthHeader_CorrectAppIdAndServiceUrl_ShouldValidate()
        {
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider("2cd87869-38a0-4182-9251-d056e8f0ac24", string.Empty);
            var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, string.Empty, "https://webchat.botframework.com/", client);

            Assert.True(result.IsAuthenticated);
        }

        [Fact]
        public async void Connector_AuthHeader_BotAppIdDiffers_ShouldNotValidate()
        {
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider("00000000-0000-0000-0000-000000000000", string.Empty);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, string.Empty, null, client));
        }

        [Fact]
        public async void Connector_AuthHeader_BotWithNoCredentials_ShouldNotValidate()
        {
            // token received and auth disabled
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider(string.Empty, string.Empty);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, string.Empty, null, client));
        }

        [Fact]
        public async void EmptyHeader_BotWithNoCredentials_ShouldThrow()
        {
            var header = string.Empty;
            var credentials = new SimpleCredentialProvider(string.Empty, string.Empty);


            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, string.Empty, null, emptyClient));
        }

        [Fact]
        public async void Emulator_MsaHeader_CorrectAppIdAndServiceUrl_ShouldValidate()
        {
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider("2cd87869-38a0-4182-9251-d056e8f0ac24", string.Empty);
            var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, string.Empty, "https://webchat.botframework.com/", emptyClient);

            Assert.True(result.IsAuthenticated);
        }

        [Fact]
        public async void Emulator_MsaHeader_BotAppIdDiffers_ShouldNotValidate()
        {
            string header = $"Bearer {await new MicrosoftAppCredentials("2cd87869-38a0-4182-9251-d056e8f0ac24", "2.30Vs3VQLKt974F").GetTokenAsync()}";
            var credentials = new SimpleCredentialProvider("00000000-0000-0000-0000-000000000000", string.Empty);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, string.Empty, null, emptyClient));            
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
                emptyClient);

            Assert.False(MicrosoftAppCredentials.IsTrustedServiceUrl("https://webchat.botframework.com/"));
        }
    }
}
