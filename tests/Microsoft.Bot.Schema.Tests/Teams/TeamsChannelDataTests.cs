// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TeamsChannelDataTests
    {
        [Fact]
        public void GetAadGroupId()
        {
            // Arrange
            const string AadGroupId = "teamGroup123";
            var activity = new Activity { ChannelData = new TeamsChannelData { Team = new TeamInfo { AadGroupId = AadGroupId } } };

            // Act
            var channelData = activity.GetChannelData<TeamsChannelData>();

            // Assert
            Assert.Equal(AadGroupId, channelData.Team.AadGroupId);
        }
    }
}
