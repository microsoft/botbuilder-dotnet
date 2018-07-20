// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Tests
{
    public static class TestMessage
    {
        public static MessageActivity Message(string id = "1234") =>
            new MessageActivity
            {
                Id = id,
                Text = "test",
                From = new ChannelAccount()
                {
                    Id = "user",
                    Name = "User Name"
                },
                Recipient = new ChannelAccount()
                {
                    Id = "bot",
                    Name = "Bot Name"
                },
                Conversation = new ConversationAccount()
                {
                    Id = "convo",
                    Name = "Convo Name"
                },
                ChannelId = "UnitTest",
                ServiceUrl = "https://example.org"
            };

    }
}
