using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Core.Tests
{
    public static class TestMessage
    {
        public static Activity Message()
        {
            Activity a = new Activity
            {
                Type = ActivityTypes.Message,
                Id = "1234",
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
