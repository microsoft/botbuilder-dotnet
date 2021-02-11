// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class AutoSaveStateMiddlewareTests
    {
        [Fact]
        public async Task AutoSaveStateMiddleware_DualReadWrite()
        {
            var storage = new MemoryStorage();

            // setup userstate
            var userState = new UserState(storage);
            var userProperty = userState.CreateProperty<int>("userCount");

            // setup convState
            var convState = new ConversationState(storage);
            var convProperty = convState.CreateProperty<int>("convCount");

            var adapter = new TestAdapter(TestAdapter.CreateConversation("AutoSaveStateMiddleware_DualReadWrite"))
                .Use(new AutoSaveStateMiddleware(userState, convState));

            const int USER_INITITAL_COUNT = 100;
            const int CONVERSATION_INITIAL_COUNT = 10;
            BotCallbackHandler botLogic = async (context, cancellationToken) =>
            {
                // get userCount and convCount from hSet
                var userCount = await userProperty.GetAsync(context, () => USER_INITITAL_COUNT).ConfigureAwait(false);
                var convCount = await convProperty.GetAsync(context, () => CONVERSATION_INITIAL_COUNT).ConfigureAwait(false);

                // System.Diagnostics.Debug.WriteLine($"{context.Activity.Id} UserCount({context.Activity.From.Id}):{userCount} convCount({context.Activity.Conversation.Id}):{convCount}");
                if (context.Activity.Type == ActivityTypes.Message)
                {
                    if (context.Activity.Text == "get userCount")
                    {
                        await context.SendActivityAsync(context.Activity.CreateReply($"{userCount}"));
                    }
                    else if (context.Activity.Text == "get convCount")
                    {
                        await context.SendActivityAsync(context.Activity.CreateReply($"{convCount}"));
                    }
                }

                // increment userCount and set property using accessor.  To be saved later by AutoSaveStateMiddleware
                userCount++;
                await userProperty.SetAsync(context, userCount);

                // increment convCount and set property using accessor.  To be saved later by AutoSaveStateMiddleware
                convCount++;
                await convProperty.SetAsync(context, convCount);
            };

            await new TestFlow(adapter, botLogic)
               .Send("test1")
                .Send("get userCount")
                    .AssertReply((USER_INITITAL_COUNT + 1).ToString())
                .Send("get userCount")
                    .AssertReply((USER_INITITAL_COUNT + 2).ToString())
                .Send("get convCount")
                    .AssertReply((CONVERSATION_INITIAL_COUNT + 3).ToString())
                .StartTestAsync();

            // new adapter on new conversation
            adapter = new TestAdapter(new ConversationReference
            {
                ChannelId = "test",
                ServiceUrl = "https://test.com",
                User = new ChannelAccount("user1", "User1"),
                Bot = new ChannelAccount("bot", "Bot"),
                Conversation = new ConversationAccount(false, "convo2", "Conversation2"),
            })
                .Use(new AutoSaveStateMiddleware(userState, convState));

            await new TestFlow(adapter, botLogic)
                .Send("get userCount")
                    .AssertReply((USER_INITITAL_COUNT + 4).ToString(), "user count should continue on new conversation")
                .Send("get convCount")
                    .AssertReply((CONVERSATION_INITIAL_COUNT + 1).ToString(), "conversationCount for conversation2 should be reset")
                .StartTestAsync();
        }

        [Fact]
        public async Task AutoSaveStateMiddleware_Chain()
        {
            var storage = new MemoryStorage();

            // setup userstate
            var userState = new UserState(storage);
            var userProperty = userState.CreateProperty<int>("userCount");

            // setup convState
            var convState = new ConversationState(storage);
            var convProperty = convState.CreateProperty<int>("convCount");
            var bss = new AutoSaveStateMiddleware()
                .Add(userState)
                .Add(convState);
            var adapter = new TestAdapter(TestAdapter.CreateConversation("AutoSaveStateMiddleware_Chain"))
                .Use(bss);

            const int USER_INITITAL_COUNT = 100;
            const int CONVERSATION_INITIAL_COUNT = 10;
            BotCallbackHandler botLogic = async (context, cancellationToken) =>
            {
                // get userCount and convCount from botStateSet
                var userCount = await userProperty.GetAsync(context, () => USER_INITITAL_COUNT).ConfigureAwait(false);
                var convCount = await convProperty.GetAsync(context, () => CONVERSATION_INITIAL_COUNT).ConfigureAwait(false);

                if (context.Activity.Type == ActivityTypes.Message)
                {
                    if (context.Activity.Text == "get userCount")
                    {
                        await context.SendActivityAsync(context.Activity.CreateReply($"{userCount}"));
                    }
                    else if (context.Activity.Text == "get convCount")
                    {
                        await context.SendActivityAsync(context.Activity.CreateReply($"{convCount}"));
                    }
                }

                // increment userCount and set property using accessor.  To be saved later by AutoSaveStateMiddleware
                userCount++;
                await userProperty.SetAsync(context, userCount);

                // increment convCount and set property using accessor.  To be saved later by AutoSaveStateMiddleware
                convCount++;
                await convProperty.SetAsync(context, convCount);
            };

            await new TestFlow(adapter, botLogic)
                .Send("test1")
                .Send("get userCount")
                    .AssertReply((USER_INITITAL_COUNT + 1).ToString())
                .Send("get userCount")
                    .AssertReply((USER_INITITAL_COUNT + 2).ToString())
                .Send("get convCount")
                    .AssertReply((CONVERSATION_INITIAL_COUNT + 3).ToString())
                .StartTestAsync();

            // new adapter on new conversation
            var bss2 = new AutoSaveStateMiddleware()
                .Add(userState)
                .Add(convState);

            adapter = new TestAdapter(new ConversationReference
            {
                ChannelId = "test",
                ServiceUrl = "https://test.com",
                User = new ChannelAccount("user1", "User1"),
                Bot = new ChannelAccount("bot", "Bot"),
                Conversation = new ConversationAccount(false, "convo2", "Conversation2"),
            })
                .Use(bss2);

            await new TestFlow(adapter, botLogic)
                .Send("get userCount")
                    .AssertReply((USER_INITITAL_COUNT + 4).ToString(), "user count should continue on new conversation")
                .Send("get convCount")
                    .AssertReply((CONVERSATION_INITIAL_COUNT + 1).ToString(), "conversationCount for conversation2 should be reset")
                .StartTestAsync();
        }
    }
}
