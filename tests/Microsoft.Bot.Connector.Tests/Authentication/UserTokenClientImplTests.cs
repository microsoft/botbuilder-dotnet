﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Moq;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class UserTokenClientImplTests
    {
        private const string AppId = "test-app-id";
        private const string ToChannelFromBotOAuthScope = "oauth-scope";
        private const string OauthEndpoint = "https://test.endpoint";
        private const string UserId = "userId";
        private const string ConnectionName = "connection-name";
        private const string ChannelId = "channel-id";
        private const string MagicCode = "magic-code";

        [Fact]
        public void ConstructorWithNullCredentialsShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new UserTokenClientImpl(AppId, null, OauthEndpoint, null, null));
        }

        [Fact]
        public void ConstructorShouldWork()
        {
            // Arrange
            //string fromBotId = "from-bot-id";
            //string toBotId = "to-bot-id";
            //string loginUrl = AuthenticationConstants.ToChannelFromBotLoginUrlTemplate;
            //Uri toUrl = new Uri("http://test1.com/test");

            //var credentialFactoryMock = new Mock<ServiceClientCredentialsFactory>();
            //credentialFactoryMock.Setup(cssf => cssf.CreateCredentialsAsync(
            //    It.Is<string>(v => v == fromBotId),
            //    It.Is<string>(v => v == toBotId),
            //    It.Is<string>(v => v == loginUrl),
            //    It.IsAny<bool>(),
            //    It.IsAny<CancellationToken>())).ReturnsAsync(MicrosoftAppCredentials.Empty);

            //var httpClientFactory = new Mock<IHttpClientFactory>();
            //httpClientFactory.Setup(x => x.CreateClient()).Returns(new HttpClient());

            var credentials = new TestCredentials();

            Assert.NotNull(new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null));
        }

        [Fact]
        public async Task GetUserTokenAsyncOfDisposedTokenShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            userToken.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await userToken.GetUserTokenAsync(UserId, ConnectionName, ChannelId, MagicCode, CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetUserTokenAsyncWithNullUserIdShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.GetUserTokenAsync(null, ConnectionName, ChannelId, MagicCode, CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetUserTokenAsyncWithNullConnectionNameShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.GetUserTokenAsync(UserId, null, ChannelId, MagicCode, CancellationToken.None);
            });
        }

        //[Fact]
        //public async Task GetUserTokenAsyncShouldReturnToken()
        //{
        //    var credentials = new TestCredentials();

        //    //var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);
        //    //var userToken = new UserTokenClientImplMockedClient(AppId, credentials, OauthEndpoint, null, null);
        //    var userToken = new Mock<UserTokenClientImpl>();
        //    userToken.SetupGet(x => x._client).Returns();

        //    var token = await userToken.GetUserTokenAsync(UserId, ConnectionName, ChannelId, MagicCode, CancellationToken.None);

        //    Assert.NotNull(token);
        //}

        [Fact]
        public async Task GetSignInResourceAsyncOfDisposedTokenShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            userToken.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await userToken.GetSignInResourceAsync(ConnectionName, new Activity(), "final-redirect", CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetSignInResourceAsyncWithNullConnectionNameShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.GetSignInResourceAsync(null, new Activity(), "final-redirect", CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetSignInResourceAsyncWithNullActivityShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.GetSignInResourceAsync(ConnectionName, null, "final-redirect", CancellationToken.None);
            });
        }

        [Fact]
        public async Task SignOutUserAsyncOfDisposedTokenShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            userToken.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await userToken.SignOutUserAsync(UserId, ConnectionName, ChannelId, CancellationToken.None);
            });
        }

        [Fact]
        public async Task SignOutUserAsyncWithNullUserIdShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.SignOutUserAsync(null, ConnectionName, ChannelId, CancellationToken.None);
            });
        }

        [Fact]
        public async Task SignOutUserAsyncWithNullConnectionNameShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.SignOutUserAsync(UserId, null, ChannelId, CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetTokenStatusAsyncOfDisposedTokenShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            userToken.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await userToken.GetTokenStatusAsync(UserId, ConnectionName, ChannelId, CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetTokenStatusAsyncWithNullUserIdShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.GetTokenStatusAsync(null, ChannelId, "filter", CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetTokenStatusAsyncWithNullChannelIdShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.GetTokenStatusAsync(UserId, null, "filter", CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetAadTokensAsyncOfDisposedTokenShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);
            
            string[] resourceUrls = { "https://test.url" };
            
            userToken.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await userToken.GetAadTokensAsync(UserId, ConnectionName, resourceUrls, ChannelId, CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetAadTokensAsyncWithNullUserIdShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            string[] resourceUrls = { "https://test.url" };

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.GetAadTokensAsync(null, ChannelId, resourceUrls, ChannelId, CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetAadTokensAsyncWithNullConnectionNameShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            string[] resourceUrls = { "https://test.url" };

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.GetAadTokensAsync(UserId, null, resourceUrls, ChannelId, CancellationToken.None);
            });
        }

        [Fact]
        public async Task ExchangeTokenAsyncOfDisposedTokenShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            var tokenExchange = new TokenExchangeRequest();

            userToken.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await userToken.ExchangeTokenAsync(UserId, ConnectionName, ChannelId, tokenExchange, CancellationToken.None);
            });
        }

        [Fact]
        public async Task ExchangeTokenAsyncWithNullUserIdShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            var tokenExchange = new TokenExchangeRequest();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.ExchangeTokenAsync(null, ChannelId, ChannelId, tokenExchange, CancellationToken.None);
            });
        }

        [Fact]
        public async Task ExchangeTokenAsyncWithNullConnectionNameShouldThrow()
        {
            var credentials = new TestCredentials();

            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);

            var tokenExchange = new TokenExchangeRequest();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await userToken.ExchangeTokenAsync(UserId, null, ChannelId, tokenExchange, CancellationToken.None);
            });
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class TestCredentials : ServiceClientCredentials
#pragma warning restore SA1402 // File may only contain a single type
    {
    }

//#pragma warning disable SA1402 // File may only contain a single type
//    internal class UserTokenClientImplMockedClient : UserTokenClientImpl
//#pragma warning restore SA1402 // File may only contain a single type
//    {
//        private static string _appId;
//        private static ServiceClientCredentials _credentials;
//        private static string _oauthEndpoint;
//        private static OAuthClient _client;
//        private static ILogger _logger;
//        private static HttpClient _httpClient;

//        public UserTokenClientImplMockedClient()
//        : base(_appId, _credentials, _oauthEndpoint, _httpClient, _logger)
//        {
//            _appId = "appId";
//            _credentials = new TestCredentials();
//            _oauthEndpoint = "oauthEndpoint";
//            _httpClient = new HttpClient();
//            ConnectorClient.AddDefaultRequestHeaders(_httpClient);

//            var tokenResponse = new TokenResponse
//            {
//                Token = "1234"
//            };

//            var userToken = new Mock<UserToken>();
//            userToken.Setup(x => x.GetTokenAsync(
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<CancellationToken>())).ReturnsAsync(tokenResponse);

//            var client = new Mock<OAuthClient>();
//            client.SetupGet(x => x.UserToken).Returns(userToken.Object);

//            _client = client.Object;
//            _logger = NullLogger.Instance;
//        }
//    }
}
