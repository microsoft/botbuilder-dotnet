// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class TestUtilities
    {
        public static TurnContext CreateEmptyContext()
        {
            var b = new TestAdapter();
            var a = new Activity
            {
                Type = ActivityTypes.Message,
                ChannelId = "EmptyContext",
                From = new ChannelAccount
                {
                    Id = "empty@empty.context.org",
                },

                Conversation = new ConversationAccount()
                {
                    Id = "213123123123",
                },
            };
            var bc = new TurnContext(b, a);

            return bc;
        }
    }
}
