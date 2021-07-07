// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TeamDetailsTests
    {
        [Fact]
        public void TeamDetailsInits()
        {
            var id = "supportEngineers";
            var name = "Support Engineers";
            var aadGroupId = "0000-0000-0000-0000-0000-0000";
            var channelCount = 1;
            var memberCount = 2;

            var teamDetails = new TeamDetails(id, name, aadGroupId)
            {
                ChannelCount = channelCount,
                MemberCount = memberCount,
            };

            Assert.NotNull(teamDetails);
            Assert.IsType<TeamDetails>(teamDetails);
            Assert.Equal(id, teamDetails.Id);
            Assert.Equal(name, teamDetails.Name);
            Assert.Equal(aadGroupId, teamDetails.AadGroupId);
            Assert.Equal(channelCount, teamDetails.ChannelCount);
            Assert.Equal(memberCount, teamDetails.MemberCount);
        }
        
        [Fact]
        public void TeamDetailsInitsWithNoArgs()
        {
            var teamDetails = new TeamDetails();

            Assert.NotNull(teamDetails);
            Assert.IsType<TeamDetails>(teamDetails);
        }
    }
}
