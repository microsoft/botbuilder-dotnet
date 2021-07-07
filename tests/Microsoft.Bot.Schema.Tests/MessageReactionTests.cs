// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class MessageReactionTests
    {
        [Fact]
        public void MessageReactionInits()
        {
            var type = "like";
            var messageReaction = new MessageReaction(type);

            Assert.NotNull(messageReaction);
            Assert.IsType<MessageReaction>(messageReaction);
            Assert.Equal(type, messageReaction.Type);
        }

        [Fact]
        public void MessageReactionInitsWithNoArgs()
        {
            var messageReaction = new MessageReaction();

            Assert.NotNull(messageReaction);
            Assert.IsType<MessageReaction>(messageReaction);
        }
    }
}
