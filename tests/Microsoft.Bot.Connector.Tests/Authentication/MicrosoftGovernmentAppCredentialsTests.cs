// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Connector.Authentication.Tests
{
    public class MicrosoftGovernmentAppCredentialsTests
    {
        [Fact]
        public void MicrosoftGovernmentAppCredentials_Has_Gov_Endpoint()
        {
            var cred = new MicrosoftGovernmentAppCredentials(string.Empty, string.Empty);

            Assert.Contains("login.microsoftonline.us", cred.OAuthEndpoint);
        }

        [Fact]
        public void MicrosoftGovernmentAppCredentials_Uses_Gov_Scope()
        {
            var cred = new MicrosoftGovernmentAppCredentials(string.Empty, string.Empty);

            Assert.Contains("api.botframework.us", cred.OAuthScope);
        }

        [Fact]
        public void GovernmentAuthenticationConstants_ChannelService_IsRight()
        {
            // This value should not change
            Assert.Equal("https://botframework.azure.us", GovernmentAuthenticationConstants.ChannelService);
        }

        [Fact]
        public void GovernmentAuthenticationConstants_ToChannelFromBotLoginUrl_IsRight()
        {
            // This value should not change
            Assert.Equal("https://login.microsoftonline.us/cab8a31a-1906-4287-a0d8-4eef66b95f6e", GovernmentAuthenticationConstants.ToChannelFromBotLoginUrl);
        }

        [Fact]
        public void GovernmentAuthenticationConstants_ToChannelFromBotOAuthScope_IsRight()
        {
            // This value should not change
            Assert.Equal("https://api.botframework.us", GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope);
        }

        [Fact]
        public void GovernmentAuthenticationConstants_ToBotFromChannelTokenIssuer_IsRight()
        {
            // This value should not change
            Assert.Equal("https://api.botframework.us", GovernmentAuthenticationConstants.ToBotFromChannelTokenIssuer);
        }

        [Fact]
        public void GovernmentAuthenticationConstants_OAuthUrlGov_IsRight()
        {
            // This value should not change
            Assert.Equal("https://api.botframework.azure.us", GovernmentAuthenticationConstants.OAuthUrlGov);
        }

        [Fact]
        public void GovernmentAuthenticationConstants_ToBotFromChannelOpenIdMetadataUrl_IsRight()
        {
            // This value should not change
            Assert.Equal("https://login.botframework.azure.us/v1/.well-known/openidconfiguration", GovernmentAuthenticationConstants.ToBotFromChannelOpenIdMetadataUrl);
        }
    }
}
