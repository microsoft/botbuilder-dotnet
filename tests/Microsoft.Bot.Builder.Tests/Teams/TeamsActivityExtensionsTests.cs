// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Builder.Teams.Tests
{
    public class TeamsActivityExtensionsTests
    {
        [Fact]
        public void TeamsGetTeamId()
        {
            // Arrange
            var activity = new Activity { ChannelData = new TeamsChannelData { Team = new TeamInfo { Id = "team123" } } };

            // Act
            var teamId = activity.TeamsGetTeamInfo().Id;

            // Assert
            Assert.Equal("team123", teamId);
        }

        [Fact]
        public void TeamsGetTeamIdTyped()
        {
            // Arrange
            IMessageActivity activity = new Activity { ChannelData = new TeamsChannelData { Team = new TeamInfo { Id = "team123" } } };

            // Act
            var teamId = activity.TeamsGetTeamInfo().Id;

            // Assert
            Assert.Equal("team123", teamId);
        }

        [Fact]
        public void TeamsNotifyUser()
        {
            // Arrange
            var activity = new Activity { };

            // Act
            activity.TeamsNotifyUser();

            // Assert
            Assert.Equal(true, ((TeamsChannelData)activity.ChannelData).Notification.Alert);
        }

        [Fact]
        public void TeamsNotifyUserExistingNotification()
        {
            // Arrange
            var activity = new Activity { ChannelData = new TeamsChannelData { Team = new TeamInfo { Id = "team123" } } };

            // Act
            activity.TeamsNotifyUser();

            // Assert
            Assert.Equal(true, ((TeamsChannelData)activity.ChannelData).Notification.Alert);
            Assert.Equal("team123", ((TeamsChannelData)activity.ChannelData).Team.Id);
        }
    }
}
