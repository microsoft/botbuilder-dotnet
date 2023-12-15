// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using Microsoft.Bot.Connector.Authentication;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
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
        public void MicrosoftGovernmentAppCredentials_Uses_Custom_Scope()
        {
            var cred = new MicrosoftGovernmentAppCredentials(string.Empty, string.Empty, null, null, "my Custom oAuthScope");

            Assert.Equal("my Custom oAuthScope", cred.OAuthScope);
        }

        [Fact]
        public void GovernmentAuthenticationConstants_ChannelService_IsRight()
        {
            // This value should not change
            Assert.Equal("https://botframework.azure.us", GovernmentAuthenticationConstants.ChannelService);
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

        [Fact]
        public void ConstructorTests()
        {
            var defaultScopeCase1 = new MicrosoftGovernmentAppCredentials("someApp", "somePassword");
            AssertEqual(defaultScopeCase1, null, null);

            var defaultScopeCase2 = new MicrosoftGovernmentAppCredentials("someApp", "somePassword", oAuthScope: "customScope");
            AssertEqual(defaultScopeCase2, null, "customScope");

            var defaultScopeCase3 = new MicrosoftGovernmentAppCredentials("someApp", "somePassword", "someTenant");
            AssertEqual(defaultScopeCase3, "someTenant", null);

            var defaultScopeCase4 = new MicrosoftGovernmentAppCredentials("someApp", "somePassword", "someTenant", oAuthScope: "customScope");
            AssertEqual(defaultScopeCase4, "someTenant", "customScope");           
        }

        private void AssertEqual(MicrosoftGovernmentAppCredentials credential, string tenantId, string oauthScope)
        {
            Assert.Equal(
                string.Format(CultureInfo.InvariantCulture, GovernmentAuthenticationConstants.ToChannelFromBotLoginUrlTemplate, credential.ChannelAuthTenant), 
                credential.OAuthEndpoint);

            if (string.IsNullOrEmpty(oauthScope))
            {
                Assert.Equal(
                    GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope,
                    credential.OAuthScope);
            }
            else
            {
                Assert.Equal(
                    oauthScope,
                    credential.OAuthScope);
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                Assert.Equal(
                    GovernmentAuthenticationConstants.DefaultChannelAuthTenant,
                    credential.ChannelAuthTenant);
            }
            else
            {
                Assert.Equal(
                tenantId,
                credential.ChannelAuthTenant);
            }
        }
    }
}
