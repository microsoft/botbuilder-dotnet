// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MeetingParticipantsRemovedEventDetailsTests
    {
        [Fact]
        public void MeetingParticipantsRemovedEventDetailsInits()
        {
            // Arrange
            var meetingId = "meetingId";
            var meetingJoinUrl = new Uri("http://meetingJoinUrl");
            var meetingTitle = "meetingTitle";
            var meetingType = "meetingType";
            var participant = new TeamsChannelAccount("id", "name", "givenName", "surname", "email", "userPrincipalName");
            var participantsRemoved = new List<TeamsChannelAccount>() { participant };

            // Act
            var meeting = new MeetingParticipantsRemovedEventDetails(meetingId, meetingJoinUrl, meetingTitle, meetingType, participantsRemoved);

            // Assert
            Assert.NotNull(meeting);
            Assert.IsType<MeetingParticipantsRemovedEventDetails>(meeting);
            Assert.Equal(meetingId, meeting.Id);
            Assert.Equal(meetingJoinUrl, meeting.JoinUrl);
            Assert.Equal(meetingTitle, meeting.Title);
            Assert.Equal(meetingType, meeting.MeetingType);
            Assert.StrictEqual(participantsRemoved, meeting.ParticipantsRemoved);
        }

        [Fact]
        public void MeetingParticipantsRemovedEventDetailsInitsWithNoArgs()
        {
            // Act
            var meeting = new MeetingParticipantsRemovedEventDetails();

            // Assert
            Assert.NotNull(meeting);
            Assert.IsType<MeetingParticipantsRemovedEventDetails>(meeting);
        }
    }
}
