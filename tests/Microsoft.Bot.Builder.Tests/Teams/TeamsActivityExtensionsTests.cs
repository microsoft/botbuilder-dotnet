// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Builder.Teams.Tests
{
    public class TeamsActivityExtensionsTests
    {
        [Fact]
        public void TeamsGetSelectedChannelId()
        {
            // Arrange
            var activity = new Activity { ChannelData = new TeamsChannelData { Settings = new TeamsChannelDataSettings { SelectedChannel = new ChannelInfo("channel123") } } };

            // Act
            var channelId = activity.TeamsGetSelectedChannelId();

            // Assert
            Assert.Equal("channel123", channelId);
        }

        [Fact]
        public void TeamsGetSelectedChannelIdNullSettings()
        {
            // Arrange
            var activity = new Activity { ChannelData = new TeamsChannelData { Settings = null } };

            // Act
            var channelId = activity.TeamsGetSelectedChannelId();

            // Assert
            Assert.Null(channelId);
        }

        [Fact]
        public void TeamsGetMeetingInfo()
        {
            // Arrange
            var activity = new Activity { ChannelData = new TeamsChannelData { Meeting = new TeamsMeetingInfo { Id = "meeting123" } } };

            // Act
            var meetingId = activity.TeamsGetMeetingInfo().Id;

            // Assert
            Assert.Equal("meeting123", meetingId);
        }

        [Fact]
        public void TeamsGetTeamId()
        {
            // Arrange
            var activity = new Activity { ChannelData = new TeamsChannelData { Team = new TeamInfo { Id = "team1234" } } };

            // Act
            var teamId = activity.TeamsGetTeamInfo().Id;

            // Assert
            Assert.Equal("team1234", teamId);
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
            Assert.Equal(false, ((TeamsChannelData)activity.ChannelData).Notification.AlertInMeeting);
        }

        [Fact]
        public void TeamsNotifyUserAlertInMeeting()
        {
            // Arrange
            var activity = new Activity { };

            // Act
            activity.TeamsNotifyUser(alertInMeeting: true);

            // Assert
            Assert.Equal(true, ((TeamsChannelData)activity.ChannelData).Notification.AlertInMeeting);
            Assert.Equal(false, ((TeamsChannelData)activity.ChannelData).Notification.Alert);
        }

        [Fact]
        public void TeamsNotifyUserExternalResourceUrl()
        {
            string resourceUrl = "https://microsoft.com";

            // Arrange
            var activity = new Activity { };

            // Act
            activity.TeamsNotifyUser(false, externalResourceUrl: resourceUrl);

            // Assert
            Assert.Equal(resourceUrl, ((TeamsChannelData)activity.ChannelData).Notification.ExternalResourceUrl);
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

        [Fact]
        public void TeamsChannelDataExistingOnBehalfOf()
        {
            // Arrange
            var onBehalfOf = new OnBehalfOf
            {
                DisplayName = "TestOnBehalfOf",
                ItemId = 0,
                MentionType = "person",
                Mri = Guid.NewGuid().ToString()
            };

            var activity = new Activity { ChannelData = new TeamsChannelData(onBehalfOf: new List<OnBehalfOf> { onBehalfOf }) };        

            // Act
            var onBehalfOfList = activity.TeamsGetTeamOnBehalfOf();

            // Assert
            Assert.Single(onBehalfOfList);
            Assert.Equal("TestOnBehalfOf", onBehalfOfList[0].DisplayName);
        }
    }
}
