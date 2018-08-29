using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Connector.Tests;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Xunit;

namespace Connector.Tests
{
    public class OAuthConnectorTests : BaseTest
    {
        private ConnectorClient mockConnectorClient = new ConnectorClient(new Uri("https://localhost"));

        [Fact]
        public void OAuthClient_ShouldThrowOnInvalidUrl()
        {
            Assert.Throws<ArgumentException>(() => new OAuthClient(mockConnectorClient, "http://localhost"));
        }

        [Fact]
        public void OAuthClient_ShouldThrowOnNullClient()
        {
            Assert.Throws<ArgumentNullException>(() => new OAuthClient(null, "https://localhost"));
        }

        [Fact]
        public async Task GetUserToken_ShouldThrowOnEmptyUserId()
        {
            var client = new OAuthClient(mockConnectorClient, "https://localhost");
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetUserTokenAsync(string.Empty, "mockConnection", string.Empty));
        }

        [Fact]
        public async Task GetUserToken_ShouldThrowOnEmptyConnectionName()
        {
            var client = new OAuthClient(mockConnectorClient, "https://localhost");
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetUserTokenAsync("userid", string.Empty, string.Empty));
        }

        [Fact]
        public async Task GetUserToken_ShouldReturnTokenWithNoMagicCode()
        {
            await UseOAuthClientFor(async client =>
             {
                 var token = await client.GetUserTokenAsync("default-user", "mygithubconnection", string.Empty);
                 Assert.NotNull(token);
                 Assert.False(string.IsNullOrEmpty(token.Token));
             });
        }

        [Fact]
        public async Task GetUserToken_ShouldReturnNullOnInvalidConnectionstring()
        {
            await UseOAuthClientFor(async client =>
             {
                 var token = await client.GetUserTokenAsync("default-user", "mygithubconnection1", string.Empty);
                 Assert.Null(token);
             });
        }

        // [Fact] - Disabled due to bug in service
        //public async Task GetSignInLinkAsync_ShouldReturnValidUrl()
        //{
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
        //}

        [Fact]
        public async Task SignOutUser_ShouldThrowOnEmptyUserId()
        {
            var client = new OAuthClient(mockConnectorClient, "https://localhost");
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.SignOutUserAsync(string.Empty, "mockConnection"));
        }

        [Fact]
        public async Task SignOutUser_ShouldThrowOnEmptyConnectionName()
        {
            var client = new OAuthClient(mockConnectorClient, "https://localhost");
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.SignOutUserAsync("userid", string.Empty));
        }

        [Fact]
        public async Task GetSigninLink_ShouldThrowOnEmptyConnectionName()
        {
            var activity = new Activity();
            var client = new OAuthClient(mockConnectorClient, "https://localhost");
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetSignInLinkAsync(activity, string.Empty));
        }

        [Fact]
        public async Task GetSigninLink_ShouldThrowOnNullActivity()
        {
            var client = new OAuthClient(mockConnectorClient, "https://localhost");
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetSignInLinkAsync(null, "mockConnection"));
        }
    }
}
