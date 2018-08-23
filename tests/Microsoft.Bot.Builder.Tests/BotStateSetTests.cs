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

            BotCallbackHandler botLogic = async (context, cancellationToken) =>
            {
                // get userCount and convCount from botStateSet
                var userCount = await userProperty.GetAsync(context, () => 100).ConfigureAwait(false);
                var convCount = await convProperty.GetAsync(context, () => 10).ConfigureAwait(false);
                            
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

                // increment userCount and save (since it's a value type, or changed object you don't need to call Set)
                userCount++;
                await userProperty.SetAsync(context, userCount);

                // increment convCount and save (since it's a value type, or changed object you don't need to call Set)
                convCount++;
                await convProperty.SetAsync(context, convCount);
            };

            await new TestFlow(adapter, botLogic)
                .Send("test1")
                .Send("get userCount")
                    .AssertReply("101")
                .Send("get userCount")
                    .AssertReply("102")
                .Send("get convCount")
                    .AssertReply("13")
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
                    .AssertReply("104", "user count should continue on new conversation")
                .Send("get convCount")
                    .AssertReply("11", "conversationCount for conversation2 should be reset")
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
