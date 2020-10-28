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
    public class ChannelServiceHandlerTests
    {
        [Fact]
        public async Task AuthenticateSetsAnonymousSkillClaim()
        {
            var sut = new TestChannelServiceHandler();
            await sut.HandleReplyToActivityAsync(null, "123", "456", new Activity(), CancellationToken.None);

            Assert.Equal(AuthenticationConstants.AnonymousAuthType, sut.ClaimsIdentity.AuthenticationType);
            Assert.Equal(AuthenticationConstants.AnonymousSkillAppId, JwtTokenValidation.GetAppIdFromClaims(sut.ClaimsIdentity.Claims));
        }

        /// <summary>
        /// A <see cref="ChannelServiceHandler"/> with overrides for testings.
        /// </summary>
        private class TestChannelServiceHandler : ChannelServiceHandler
        {
            public TestChannelServiceHandler()
                : base(new SimpleCredentialProvider(), new AuthenticationConfiguration())
            {
            }

            /// <summary>
            /// Gets the <see cref="ClaimsIdentity"/> sent to the different methods after auth is done.
            /// </summary>
            public ClaimsIdentity ClaimsIdentity { get; private set; }

            protected override Task<ResourceResponse> OnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
            {
                ClaimsIdentity = claimsIdentity;
                return Task.FromResult(new ResourceResponse());
            }
        }
    }
}
