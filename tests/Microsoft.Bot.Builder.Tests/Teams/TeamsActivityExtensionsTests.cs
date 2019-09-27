// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Teams.Tests
{
    [TestClass]
    public class TeamsActivityExtensionsTests
    {
        [TestMethod]
        public void TeamsGetTeamId()
        {
            // Arrange
            var activity = new Activity { ChannelData = new TeamsChannelData { Team = new TeamInfo { Id = "team123" } } };

            // Act
            var teamId = activity.TeamsGetTeamId();

            // Assert
            Assert.AreEqual("team123", teamId);
        }

        [TestMethod]
        public void TeamsGetTeamIdTyped()
        {
            // Arrange
            IMessageActivity activity = new Activity { ChannelData = new TeamsChannelData { Team = new TeamInfo { Id = "team123" } } };

            // Act
            var teamId = activity.TeamsGetTeamId();

            // Assert
            Assert.AreEqual("team123", teamId);
        }

        [TestMethod]
        public void TeamsNotifyUser()
        {
            // Arrange
            var activity = new Activity { };

            // Act
            activity.TeamsNotifyUser();

            // Assert
            Assert.AreEqual(true, ((TeamsChannelData)activity.ChannelData).Notification.Alert);
        }

        [TestMethod]
        public void TeamsNotifyUserExistingNotification()
        {
            // Arrange
            var activity = new Activity { ChannelData = new TeamsChannelData { Team = new TeamInfo { Id = "team123" } } };

            // Act
            activity.TeamsNotifyUser();

            // Assert
            Assert.AreEqual(true, ((TeamsChannelData)activity.ChannelData).Notification.Alert);
            Assert.AreEqual("team123", ((TeamsChannelData)activity.ChannelData).Team.Id);
        }
    }
}
