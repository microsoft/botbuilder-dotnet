// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TeamsMeetingInfoTests
    {
        [Fact]
        public void TeamsMeetingInfoInits()
        {
            var id = "BFSE Stand Up";

            var meetingInfo = new TeamsMeetingInfo(id);

            Assert.NotNull(meetingInfo);
            Assert.IsType<TeamsMeetingInfo>(meetingInfo);
            Assert.Equal(id, meetingInfo.Id);
        }
        
        [Fact]
        public void TeamsMeetingInfoInitsWithNoArgs()
        {
            var meetingInfo = new TeamsMeetingInfo();

            Assert.NotNull(meetingInfo);
            Assert.IsType<TeamsMeetingInfo>(meetingInfo);
        }
    }
}
