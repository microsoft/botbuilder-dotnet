// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("State Management")]
    public class BotStateSetTests
    {
        [TestMethod]
        public async Task BotStateSet_DualReadWrite()
        {
            var storage = new MemoryStorage();

            // setup userstate
            var userState = new UserState(storage);
            var userProperty = userState.CreateProperty<int>("userCount");

            // setup convState
            var convState = new ConversationState(storage);
            var convProperty = convState.CreateProperty<int>("convCount");

            var adapter = new TestAdapter()
                .Use(new BotStateSet(userState, convState));

            const int USER_INITITAL_COUNT = 100;
            const int CONVERSATION_INITIAL_COUNT = 10;
            BotCallbackHandler botLogic = async (context, cancellationToken) =>
            {
                // get userCount and convCount from botStateSet
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

                // increment userCount and set property using accessor.  To be saved later by BotStateSet
                userCount++;
                await userProperty.SetAsync(context, userCount);

                // increment convCount and set property using accessor.  To be saved later by BotStateSet
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
                Conversation = new ConversationAccount(false, "convo2", "Conversation2")
            })
                .Use(new BotStateSet(userState, convState));

            await new TestFlow(adapter, botLogic)
                .Send("get userCount")
                    .AssertReply((USER_INITITAL_COUNT + 4).ToString(), "user count should continue on new conversation")
                .Send("get convCount")
                    .AssertReply((CONVERSATION_INITIAL_COUNT + 1).ToString(), "conversationCount for conversation2 should be reset")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task BotStateSet_Chain()
        {
            var storage = new MemoryStorage();

            // setup userstate
            var userState = new UserState(storage);
            var userProperty = userState.CreateProperty<int>("userCount");

            // setup convState
            var convState = new ConversationState(storage);
            var convProperty = convState.CreateProperty<int>("convCount");
            var bss = new BotStateSet()
                .Use(userState)
                .Use(convState);
            var adapter = new TestAdapter()
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

                // increment userCount and set property using accessor.  To be saved later by BotStateSet
                userCount++;
                await userProperty.SetAsync(context, userCount);

                // increment convCount and set property using accessor.  To be saved later by BotStateSet
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
            var bss2 = new BotStateSet()
                .Use(userState)
                .Use(convState);

            adapter = new TestAdapter(new ConversationReference
            {
                ChannelId = "test",
                ServiceUrl = "https://test.com",
                User = new ChannelAccount("user1", "User1"),
                Bot = new ChannelAccount("bot", "Bot"),
                Conversation = new ConversationAccount(false, "convo2", "Conversation2")
            })
                .Use(bss2);

            await new TestFlow(adapter, botLogic)
                .Send("get userCount")
                    .AssertReply((USER_INITITAL_COUNT + 4).ToString(), "user count should continue on new conversation")
                .Send("get convCount")
                    .AssertReply((CONVERSATION_INITIAL_COUNT + 1).ToString(), "conversationCount for conversation2 should be reset")
                .StartTestAsync();

        }


        [TestMethod]
        public void BotStateSet_Properties()
        {
            var storage = new MemoryStorage();

            // setup userstate
            var userState = new UserState(storage);
            var userProperty = userState.CreateProperty<int>("userCount");

            // setup convState
            var convState = new ConversationState(storage);
            var convProperty = convState.CreateProperty<int>("convCount");

            var stateSet = new BotStateSet(userState, convState);

            Assert.AreEqual(stateSet.BotStates.Count, 2);
            Assert.IsNotNull(stateSet.BotStates.OfType<UserState>().First());
            Assert.IsNotNull(stateSet.BotStates.OfType<ConversationState>().First());
        }

    }
}
