// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MessageActionsPayloadMentionTests
    {
        [Fact]
        public void MessageActionsPayloadMentionInits()
        {
            var id = 123;
            var mentionText = "Sam-I-Am";
            var mentioned = new MessageActionsPayloadFrom(new MessageActionsPayloadUser("aadUser", id.ToString()));

            var msgActionsPayloadMention = new MessageActionsPayloadMention(id, mentionText, mentioned);

            Assert.NotNull(msgActionsPayloadMention);
            Assert.IsType<MessageActionsPayloadMention>(msgActionsPayloadMention);
            Assert.Equal(id, msgActionsPayloadMention.Id);
            Assert.Equal(mentionText, msgActionsPayloadMention.MentionText);
            Assert.Equal(mentioned, msgActionsPayloadMention.Mentioned);
        }
        
        [Fact]
        public void MessageActionsPayloadMentionInitsWithNoArgs()
        {
            var msgActionsPayloadMention = new MessageActionsPayloadMention();

            Assert.NotNull(msgActionsPayloadMention);
            Assert.IsType<MessageActionsPayloadMention>(msgActionsPayloadMention);
        }
    }
}
