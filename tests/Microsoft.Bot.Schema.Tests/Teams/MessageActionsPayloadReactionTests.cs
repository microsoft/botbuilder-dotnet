// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MessageActionsPayloadReactionTests
    {
        [Fact]
        public void MessageActionsPayloadReactionInits()
        {
            var reactionType = "heart";
            var createdDateTime = "2009-06-15T13:45:30";
            var user = new MessageActionsPayloadFrom(new MessageActionsPayloadUser("aadUser", "1234", "Jane Doe"));

            var msgActionsPayloadReaction = new MessageActionsPayloadReaction(reactionType, createdDateTime, user);

            Assert.NotNull(msgActionsPayloadReaction);
            Assert.IsType<MessageActionsPayloadReaction>(msgActionsPayloadReaction);
            Assert.Equal(reactionType, msgActionsPayloadReaction.ReactionType);
            Assert.Equal(createdDateTime, msgActionsPayloadReaction.CreatedDateTime);
            Assert.Equal(user, msgActionsPayloadReaction.User);
        }
        
        [Fact]
        public void MessageActionsPayloadReactionInitsWithNoArgs()
        {
            var msgActionsPayloadReaction = new MessageActionsPayloadReaction();

            Assert.NotNull(msgActionsPayloadReaction);
            Assert.IsType<MessageActionsPayloadReaction>(msgActionsPayloadReaction);
        }
    }
}
