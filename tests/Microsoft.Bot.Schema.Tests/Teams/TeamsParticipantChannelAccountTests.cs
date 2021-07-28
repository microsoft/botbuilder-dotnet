// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TeamsParticipantChannelAccountTests
    {
        [Fact]
        public void TeamsParticipantChannelAccountInits()
        {
            var id = "joe@smith.com";
            var name = "Joe Smith";
            var givenName = "Joe";
            var surname = "Smith";
            var email = "joe@smith.com";
            var userPrincipalName = "joeUserPrincipalName";
            var tenantId = "123456-7890-abcd-efgh-ijklmnop";
            var userRole = "owner";
            var meetingRole = "owner";
            var inMeeting = true;
            var conversation = new ConversationAccount(true);

            var participantAccount = new TeamsParticipantChannelAccount(id, name, givenName, surname, email, userPrincipalName, tenantId, userRole, meetingRole, inMeeting, conversation);

            Assert.NotNull(participantAccount);
            Assert.IsType<TeamsParticipantChannelAccount>(participantAccount);
            Assert.Equal(id, participantAccount.Id);
            Assert.Equal(name, participantAccount.Name);
            Assert.Equal(givenName, participantAccount.GivenName);
            Assert.Equal(surname, participantAccount.Surname);
            Assert.Equal(email, participantAccount.Email);
            Assert.Equal(userPrincipalName, participantAccount.UserPrincipalName);
            Assert.Equal(tenantId, participantAccount.TenantId);
            Assert.Equal(userRole, participantAccount.UserRole);
            Assert.Equal(meetingRole, participantAccount.MeetingRole);
            Assert.Equal(inMeeting, participantAccount.InMeeting);
            Assert.Equal(conversation, participantAccount.Conversation);
        }
        
        [Fact]
        public void TeamsParticipantChannelAccountInitsWithNoArgs()
        {
            var participantAccount = new TeamsParticipantChannelAccount();

            Assert.NotNull(participantAccount);
            Assert.IsType<TeamsParticipantChannelAccount>(participantAccount);
        }
    }
}
