// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class PagedMemberResultTests
    {
        [Fact]
        public void PagedMemberResultInits()
        {
            var continuationToken = "myContinuationToken";
            var members = new List<ChannelAccount>()
            { 
                new ChannelAccount("id1", "channel1", "role1", "aadObjectId1"),
                new ChannelAccount("id2", "channel2", "role2", "aadObjectId2"),
            };

            var pagedMemberResult = new PagedMembersResult(continuationToken, members);

            Assert.NotNull(pagedMemberResult);
            Assert.IsType<PagedMembersResult>(pagedMemberResult);
            Assert.Equal(continuationToken, pagedMemberResult.ContinuationToken);
            Assert.Equal(members, pagedMemberResult.Members);
        }

        [Fact]
        public void PagedMemberResultInitsWithNoArgs()
        {
            var pagedMemberResult = new PagedMembersResult();

            Assert.NotNull(pagedMemberResult);
            Assert.IsType<PagedMembersResult>(pagedMemberResult);
        }
    }
}
