// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Tests
{
    public static class TestMessage
    {
        public static Activity Message(string id = "1234")
        {
            Activity a = new Activity
            {
                Type = ActivityTypes.Message,
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
            return a;
        }

    }
}
