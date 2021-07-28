// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TeamsChannelAccountTests
    {
        [Fact]
        public void TeamChannelAccountInits()
        {
            var id = "john@smith.com";
            var name = "John Smith";
            var givenName = "John";
            var surname = "Smith";
            var email = "john@smith.com";
            var userPrincipalName = "johnUserPrincipal";

            var teamsChannelAccount = new TeamsChannelAccount(id, name, givenName, surname, email, userPrincipalName);

            Assert.NotNull(teamsChannelAccount);
            Assert.IsType<TeamsChannelAccount>(teamsChannelAccount);
            Assert.Equal(id, teamsChannelAccount.Id);
            Assert.Equal(name, teamsChannelAccount.Name);
            Assert.Equal(givenName, teamsChannelAccount.GivenName);
            Assert.Equal(surname, teamsChannelAccount.Surname);
            Assert.Equal(email, teamsChannelAccount.Email);
            Assert.Equal(userPrincipalName, teamsChannelAccount.UserPrincipalName);
        }
        
        [Fact]
        public void TeamChannelAccountInitsWithAllOptions()
        {
            var id = "john@smith.com";
            var name = "John Smith";
            var givenName = "John";
            var surname = "Smith";
            var email = "john@smith.com";
            var userPrincipalName = "johnUserPrincipal";
            var tenantId = "0000-0000-0000-0000-0000-0000";
            var userRole = "contributor";
            var objectId = "abcdefgh-ijkl-mnop-qrst-uvwxyz123456";

            var teamsChannelAccount = new TeamsChannelAccount(id, name, givenName, surname, email, userPrincipalName, tenantId, userRole)
            {
                AadObjectId = objectId
            };

            Assert.NotNull(teamsChannelAccount);
            Assert.IsType<TeamsChannelAccount>(teamsChannelAccount);
            Assert.Equal(id, teamsChannelAccount.Id);
            Assert.Equal(name, teamsChannelAccount.Name);
            Assert.Equal(givenName, teamsChannelAccount.GivenName);
            Assert.Equal(surname, teamsChannelAccount.Surname);
            Assert.Equal(email, teamsChannelAccount.Email);
            Assert.Equal(userPrincipalName, teamsChannelAccount.UserPrincipalName);
            Assert.Equal(tenantId, teamsChannelAccount.TenantId);
            Assert.Equal(userRole, teamsChannelAccount.UserRole);
            Assert.Equal(objectId, teamsChannelAccount.AadObjectId);
        }
        
        [Fact]
        public void TeamChannelAccountInitsWithNoArgs()
        {
            var teamsChannelAccount = new TeamsChannelAccount();

            Assert.NotNull(teamsChannelAccount);
            Assert.IsType<TeamsChannelAccount>(teamsChannelAccount);
        }
    }
}
