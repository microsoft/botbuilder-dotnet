// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class AppCredentialsTests
    {
        [Fact]
        public void ConstructorTests()
        {
            var shouldDefaultToChannelScope = new TestAppCredentials("irrelevant", null, null);
            Assert.Equal(AuthenticationConstants.ToChannelFromBotOAuthScope, shouldDefaultToChannelScope.OAuthScope);

            var shouldDefaultToCustomScope = new TestAppCredentials("irrelevant", null, null, "customScope");
            Assert.Equal("customScope", shouldDefaultToCustomScope.OAuthScope);
        }

        [Fact]
        public async Task ProcessHttpRequestShouldNotSendTokenForAnonymous()
        {
            var sut = new TestAppCredentials("irrelevant", null, null)
            {
                MicrosoftAppId = null
            };

            // AppId is null.
            await sut.ProcessHttpRequestAsync(new HttpRequestMessage(), CancellationToken.None);
            Assert.Null(sut.Request.Headers.Authorization);

            // AppId is anonymous skill.
            sut.MicrosoftAppId = AuthenticationConstants.AnonymousSkillAppId;
            Assert.Null(sut.Request.Headers.Authorization);
        }

        private class TestAppCredentials : AppCredentials
        {
            public TestAppCredentials(string channelAuthTenant = null, HttpClient customHttpClient = null, ILogger logger = null)
                : base(channelAuthTenant, customHttpClient, logger)
            {
            }

            public TestAppCredentials(string channelAuthTenant = null, HttpClient customHttpClient = null, ILogger logger = null, string oAuthScope = null)
                : base(channelAuthTenant, customHttpClient, logger, oAuthScope)
            {
            }

            /// <summary>
            /// Gets the request sent to <see cref="ProcessHttpRequestAsync"/>.
            /// </summary>
            public HttpRequestMessage Request { get; private set; }

            public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // This override calls the base and captures the modified request so we can assert the Auth header.
                await base.ProcessHttpRequestAsync(request, cancellationToken);
                Request = request;
            }

            protected override Lazy<AdalAuthenticator> BuildAuthenticator()
            {
                return new Mock<Lazy<AdalAuthenticator>>().Object;
            }
        }
    }
}
