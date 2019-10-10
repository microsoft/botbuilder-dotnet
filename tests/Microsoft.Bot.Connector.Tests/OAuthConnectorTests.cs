// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Xunit;

namespace Microsoft.Bot.Connector.Tests
{
    public class OAuthConnectorTests : BaseTest
    {
        [Fact]
        public void OAuthClient_ShouldNotThrowOnHttpUrl()
        {
            var client = new OAuthClient(new Uri("http://localhost"), new BotAccessTokenStub("token"));
            Assert.NotNull(client);
        }

        [Fact]
        public void OAuthClient_ShouldThrowOnNullCredentials()
        {
            Assert.Throws<ArgumentNullException>(() => new OAuthClient(new Uri("https://localhost"), null));
        }

        [Fact]
        public async Task GetUserToken_ShouldThrowOnEmptyUserId()
        {
            var client = new OAuthClient(new Uri("http://localhost"), new BotAccessTokenStub("token"));
            await Assert.ThrowsAsync<ValidationException>(() => client.UserToken.GetTokenAsync(null, "mockConnection", string.Empty));
        }

        [Fact]
        public async Task GetUserToken_ShouldThrowOnEmptyConnectionName()
        {
            var client = new OAuthClient(new Uri("http://localhost"), new BotAccessTokenStub("token"));
            await Assert.ThrowsAsync<ValidationException>(() => client.UserToken.GetTokenAsync("userid", null, string.Empty));
        }

        // [Fact] - Disabled due to bug in service
        // public async Task GetUserToken_ShouldReturnTokenWithNoMagicCode()
        // {
        //    await UseOAuthClientFor(async client =>
        //     {
        //         var token = await client.UserToken.GetTokenAsync("default-user", "mygithubconnection", null, null);
        //         Assert.NotNull(token);
        //         Assert.False(string.IsNullOrEmpty(token.Token));
        //     });
        // }
        [Fact]
        public async Task GetUserToken_ShouldReturnNullOnInvalidConnectionString()
        {
            await UseOAuthClientFor(async client =>
             {
                 var token = await client.UserToken.GetTokenAsync("default-user", "mygithubconnection1", null, null);
                 Assert.Null(token);
             });
        }

        [Fact]
        public async Task SignOutUser_ShouldThrowOnEmptyUserId()
        {
            var client = new OAuthClient(new Uri("http://localhost"), new BotAccessTokenStub("token"));
            await Assert.ThrowsAsync<ValidationException>(() => client.UserToken.SignOutAsync(null, "mockConnection"));
        }

        [Fact]
        public async Task GetSigninLink_ShouldThrowOnNullState()
        {
            var client = new OAuthClient(new Uri("http://localhost"), new BotAccessTokenStub("token"));
            await Assert.ThrowsAsync<ValidationException>(() => client.BotSignIn.GetSignInUrlAsync(null));
        }

        [Fact]
        public async Task GetTokenStatus_ShouldThrowOnNullUserId()
        {
            var client = new OAuthClient(new Uri("http://localhost"), new BotAccessTokenStub("token"));
            await Assert.ThrowsAsync<ValidationException>(() => client.UserToken.GetTokenStatusAsync(null));
        }

        [Fact]
        public async Task GetAadTokensAsync_ShouldThrowOnNullUserId()
        {
            var client = new OAuthClient(new Uri("http://localhost"), new BotAccessTokenStub("token"));
            await Assert.ThrowsAsync<ValidationException>(() => client.UserToken.GetAadTokensAsync(null, "connection", new AadResourceUrls() { ResourceUrls = new string[] { "hello" } }));
        }

        [Fact]
        public async Task GetAadTokensAsync_ShouldThrowOnNullConncetionName()
        {
            var client = new OAuthClient(new Uri("http://localhost"), new BotAccessTokenStub("token"));
            await Assert.ThrowsAsync<ValidationException>(() => client.UserToken.GetAadTokensAsync("user", null, new AadResourceUrls() { ResourceUrls = new string[] { "hello" } }));
        }

        [Fact]
        public async Task GetAadTokensAsync_ShouldThrowOnNullResourceUrls()
        {
            var client = new OAuthClient(new Uri("http://localhost"), new BotAccessTokenStub("token"));
            await Assert.ThrowsAsync<ValidationException>(() => client.UserToken.GetAadTokensAsync("user", "connection", null));
        }

        // [Fact] - Disabled due to bug in service
        // public async Task GetSignInLinkAsync_ShouldReturnValidUrl()
        // {
        //    var activity = new Activity()
        //    {
        //        Id = "myid",
        //        From = new ChannelAccount() { Id = "fromId" },
        //        ServiceUrl = "https://localhost"
        //    };
        //    await UseOAuthClientFor(async client =>
        //     {
        //         var uri = await client.GetSignInLinkAsync(activity, "mygithubconnection");
        //         Assert.False(string.IsNullOrEmpty(uri));
        //         Uri uriResult;
        //         Assert.True(Uri.TryCreate(uri, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttps);
        //     });
        // }
    }
}
