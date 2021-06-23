// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TeamsChannelDataTests
    {
        [Fact]
        public void TeamsChannelDataInits()
        {
            var channel = new ChannelInfo("general", "General");
            var eventType = "eventType";
            var team = new TeamInfo("supportEngineers", "Support Engineers");
            var notification = new NotificationInfo(true);
            var tenant = new TenantInfo("uniqueTenantId");
            var meeting = new TeamsMeetingInfo("BFSE Stand Up");

            var channelData = new TeamsChannelData(channel, eventType, team, notification, tenant)
            {
                Meeting = meeting
            };

            Assert.NotNull(channelData);
            Assert.IsType<TeamsChannelData>(channelData);
            Assert.Equal(channel, channelData.Channel);
            Assert.Equal(eventType, channelData.EventType);
            Assert.Equal(team, channelData.Team);
            Assert.Equal(notification, channelData.Notification);
            Assert.Equal(tenant, channelData.Tenant);
            Assert.Equal(meeting, channelData.Meeting);
        }
        
        [Fact]
        public void TeamsChannelDataInitsWithNoArgs()
        {
            var channelData = new TeamsChannelData();

            Assert.NotNull(channelData);
            Assert.IsType<TeamsChannelData>(channelData);
        }

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
