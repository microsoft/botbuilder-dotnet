// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using Microsoft.Bot.Connector.Client.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Bot.Connector.Client.Tests.Authentication
{
    public class MicrosoftAppCredentialsTests
    {
        [Fact]
        public void ConstructorTests()
        {
            var defaultScopeCase1 = new MicrosoftAppCredentials("someApp", "somePassword");
            Assert.Equal(AuthenticationConstants.ToChannelFromBotOAuthScope, defaultScopeCase1.OAuthScope);

            using (var customHttpClient = new HttpClient())
            {
                // Use with default scope
                var defaultScopeCase2 = new MicrosoftAppCredentials("someApp", "somePassword", customHttpClient);
                Assert.Equal(AuthenticationConstants.ToChannelFromBotOAuthScope, defaultScopeCase2.OAuthScope);

                var logger = new Mock<ILogger>().Object;
                var defaultScopeCase3 = new MicrosoftAppCredentials("someApp", "somePassword", customHttpClient, logger);
                Assert.Equal(AuthenticationConstants.ToChannelFromBotOAuthScope, defaultScopeCase3.OAuthScope);

                var defaultScopeCase4 = new MicrosoftAppCredentials("someApp", "somePassword", "someTenant", customHttpClient);
                Assert.Equal(AuthenticationConstants.ToChannelFromBotOAuthScope, defaultScopeCase4.OAuthScope);

                var defaultScopeCase5 = new MicrosoftAppCredentials("someApp", "somePassword", "someTenant", customHttpClient, logger);
                Assert.Equal(AuthenticationConstants.ToChannelFromBotOAuthScope, defaultScopeCase5.OAuthScope);

                // Use custom scope
                var customScopeCase1 = new MicrosoftAppCredentials("someApp", "somePassword", customHttpClient, logger, "customScope");
                Assert.Equal("customScope", customScopeCase1.OAuthScope);

                var customScopeCase2 = new MicrosoftAppCredentials("someApp", "somePassword", "someTenant", customHttpClient, logger, "customScope");
                Assert.Equal("customScope", customScopeCase2.OAuthScope);
            }
        }
    }
}
