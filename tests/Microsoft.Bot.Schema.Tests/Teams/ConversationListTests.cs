// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class ConversationListTests
    {
        [Fact]
        public void ConversationListInits()
        {
            var conversations = new List<ChannelInfo>()
            {
                new ChannelInfo("123", "watercooler"),
                new ChannelInfo("456", "release train"),
            };
            var conversationList = new ConversationList(conversations);

            Assert.NotNull(conversationList);
            Assert.IsType<ConversationList>(conversationList);
            Assert.Equal(conversations, conversationList.Conversations);
            Assert.Equal(conversations.Count, conversationList.Conversations.Count);
            Assert.Equal(conversations[0].Id, conversationList.Conversations[0].Id);
            Assert.Equal(conversations[1].Id, conversationList.Conversations[1].Id);
        }
        
        [Fact]
        public void ConversationListInitsWithNoArgs()
        {
            var conversationList = new ConversationList();

            Assert.NotNull(conversationList);
            Assert.IsType<ConversationList>(conversationList);
        }
    }
}
