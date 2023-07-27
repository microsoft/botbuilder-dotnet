// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MeetingParticipantsAddedEventDetailsTests
    {
        [Fact]
        public void MeetingParticipantsAddedEventDetailsInits()
        {
            // Arrange
            var meetingId = "meetingId";
            var meetingJoinUrl = new Uri("http://meetingJoinUrl");
            var meetingTitle = "meetingTitle";
            var meetingType = "meetingType";
            var participant = new TeamsChannelAccount("id", "name", "givenName", "surname", "email", "userPrincipalName");
            var participantsAdded = new List<TeamsChannelAccount>() { participant };

            // Act
            var meeting = new MeetingParticipantsAddedEventDetails(meetingId, meetingJoinUrl, meetingTitle, meetingType, participantsAdded);

            // Assert
            Assert.NotNull(meeting);
            Assert.IsType<MeetingParticipantsAddedEventDetails>(meeting);
            Assert.Equal(meetingId, meeting.Id);
            Assert.Equal(meetingJoinUrl, meeting.JoinUrl);
            Assert.Equal(meetingTitle, meeting.Title);
            Assert.Equal(meetingType, meeting.MeetingType);
            Assert.StrictEqual(participantsAdded, meeting.ParticipantsAdded);
        }

        [Fact]
        public void MeetingParticipantsAddedEventDetailsInitsWithNoArgs()
        {
            // Act
            var meeting = new MeetingParticipantsAddedEventDetails();

            // Assert
            Assert.NotNull(meeting);
            Assert.IsType<MeetingParticipantsAddedEventDetails>(meeting);
        }
    }
}
