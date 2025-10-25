// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using Microsoft.Bot.Connector.Authentication;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class MicrosoftAppCredentialsTests
    {
        [Fact]
        public void ConstructorTests()
        {
            var defaultScopeCase1 = new MicrosoftAppCredentials(null, "someApp", "somePassword");
            AssertEqual(defaultScopeCase1, null, null);

            var defaultScopeCase2 = new MicrosoftAppCredentials(null, "someApp", "somePassword", oAuthScope: "customScope");
            AssertEqual(defaultScopeCase2, null, "customScope");

            var defaultScopeCase3 = new MicrosoftAppCredentials(null, "someApp", "somePassword", "someTenant");
            AssertEqual(defaultScopeCase3, "someTenant", null);

            var defaultScopeCase4 = new MicrosoftAppCredentials(null, "someApp", "somePassword", "someTenant", oAuthScope: "customScope");
            AssertEqual(defaultScopeCase4, "someTenant", "customScope");
        }

        private void AssertEqual(MicrosoftAppCredentials credential, string tenantId, string oauthScope)
        {
            Assert.Equal(
                string.Format(CultureInfo.InvariantCulture, AuthenticationConstants.ToChannelFromBotLoginUrlTemplate, credential.ChannelAuthTenant),
                credential.OAuthEndpoint);

            if (string.IsNullOrEmpty(oauthScope))
            {
                Assert.Equal(
                    AuthenticationConstants.ToChannelFromBotOAuthScope,
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
                    AuthenticationConstants.DefaultChannelAuthTenant,
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
