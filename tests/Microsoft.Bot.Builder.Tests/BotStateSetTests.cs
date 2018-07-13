// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
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
            var userProperty = userState.CreateProperty<int>("userCount", () => 100);

            // setup convState
            var convState = new ConversationState(storage);
            var convProperty = convState.CreateProperty<int>("convCount", () => 10);

            var adapter = new TestAdapter()
                .Use(new BotStateSet(userState, convState));

            Func<ITurnContext, Task> botLogic = async (context) =>
                        {
                            // get userCount and convCount from botStateSet
                            var userCount = await userProperty.GetAsync(context).ConfigureAwait(false);
                            var convCount = await convProperty.GetAsync(context).ConfigureAwait(false);
                            
                            // System.Diagnostics.Debug.WriteLine($"{context.Activity.Id} UserCount({context.Activity.From.Id}):{userCount} convCount({context.Activity.Conversation.Id}):{convCount}");

                            if (context.Activity is MessageActivity message)
                            {
                                if (message.Text == "get userCount")
                                {
                                    await context.SendActivityAsync(context.Activity.CreateReply($"{userCount}"));
                                }
                                else if (message.Text == "get convCount")
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

    }
}
