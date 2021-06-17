// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class UserTokenClientImplTests
    {
        private const string AppId = "test-app-id";
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

        [Fact]
        public void DisposeOfDisposedTokenShouldReturn()
        {
            var credentials = new TestCredentials();
            var userToken = new UserTokenClientImpl(AppId, credentials, OauthEndpoint, null, null);
            userToken.Dispose();
            userToken.Dispose();
        }

        private class TestCredentials : ServiceClientCredentials
        {
        }
    }
}
