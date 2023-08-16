// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MeetingParticipantsEventDetailsTests
    {
        [Fact]
        public void MeetingParticipantsEventDetailsInits()
        {
            // Arrange
            var user = new TeamsChannelAccount("id", "name", "givenName", "surname", "email", "userPrincipalName");
            var member = new TeamsMeetingMember(user, new UserMeetingDetails { InMeeting = true, Role = "user" });
            var eventMembers = new List<TeamsMeetingMember>() { member };

            // Act
            var meeting = new MeetingParticipantsEventDetails(eventMembers);

            // Assert
            Assert.NotNull(meeting);
            Assert.IsType<MeetingParticipantsEventDetails>(meeting);

            Assert.StrictEqual(eventMembers, meeting.Members);
        }

        [Fact]
        public void MeetingParticipantsEventDetailsInitsWithNoArgs()
        {
            // Act
            var meeting = new MeetingParticipantsEventDetails();

            // Assert
            Assert.NotNull(meeting);
            Assert.IsType<MeetingParticipantsEventDetails>(meeting);
        }
    }
}
