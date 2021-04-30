// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class ConversationTests
    {
        [Fact]
        public void ConversationAccountInits()
        {
            var isGroup = true;
            var conversationType = "convoType";
            var id = "myId";
            var name = "name";
            var aadObjectId = "aadObjectId";
            var role = "role";
            var tenantId = "tenantId";
            var props = new JObject();

            var convoAccount = new ConversationAccount(isGroup, conversationType, id, name, aadObjectId, role, tenantId)
            {
                Properties = props
            };

            Assert.NotNull(convoAccount);
            Assert.IsType<ConversationAccount>(convoAccount);
            Assert.Equal(isGroup, convoAccount.IsGroup);
            Assert.Equal(conversationType, convoAccount.ConversationType);
            Assert.Equal(id, convoAccount.Id);
            Assert.Equal(name, convoAccount.Name);
            Assert.Equal(aadObjectId, convoAccount.AadObjectId);
            Assert.Equal(role, convoAccount.Role);
            Assert.Equal(tenantId, convoAccount.TenantId);
            Assert.Equal(props, convoAccount.Properties);
        }

        [Fact]
        public void ConversationMembersInits()
        {
            var id = "myId";
            var members = new List<ChannelAccount>() { new ChannelAccount("id", "name", "role", "aadObjectId") };

            var convoMembers = new ConversationMembers(id, members);

            Assert.NotNull(convoMembers);
            Assert.IsType<ConversationMembers>(convoMembers);
            Assert.Equal(id, convoMembers.Id);
            Assert.Equal(members, convoMembers.Members);
        }

        [Fact]
        public void ConversationMembersInitsWithNoArgs()
        {
            var convoMembers = new ConversationMembers();

            Assert.NotNull(convoMembers);
            Assert.IsType<ConversationMembers>(convoMembers);
        }
    }
}
