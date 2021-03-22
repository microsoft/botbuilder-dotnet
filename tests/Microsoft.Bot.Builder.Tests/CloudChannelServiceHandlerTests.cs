// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class CloudChannelServiceHandlerTests
    {
        [Fact]
        public async Task AuthenticateSetsAnonymousSkillClaim()
        {
            var sut = new TestCloudChannelServiceHandler(BotFrameworkAuthenticationFactory.Create());
            await sut.HandleReplyToActivityAsync(null, "123", "456", new Activity(), CancellationToken.None);

            Assert.Equal(AuthenticationConstants.AnonymousAuthType, sut.ClaimsIdentity.AuthenticationType);
            Assert.Equal(AuthenticationConstants.AnonymousSkillAppId, JwtTokenValidation.GetAppIdFromClaims(sut.ClaimsIdentity.Claims));
        }

        private class TestCloudChannelServiceHandler : CloudChannelServiceHandler
        {
            public TestCloudChannelServiceHandler(BotFrameworkAuthentication auth)
                : base(auth)
            {
            }

            public ClaimsIdentity ClaimsIdentity { get; private set; }

            protected override Task<ResourceResponse> OnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
            {
                ClaimsIdentity = claimsIdentity;
                return Task.FromResult(new ResourceResponse());
            }
        }
    }
}
