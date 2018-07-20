// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    public class TestState : IStoreItem
    {
        public string ETag { get; set; }
        public string Value { get; set; }
    }

    public class TestPocoState
    {
        public string Value { get; set; }
    }

    [TestClass]
    [TestCategory("State Management")]
    public class BotStateTests
    {
        [TestMethod]
        public async Task State_DoNOTRememberContextState()
        {

            TestAdapter adapter = new TestAdapter();

            await new TestFlow(adapter, (context) =>
                   {
                       var obj = context.Services.Get<UserState>();
                       Assert.IsNull(obj, "context.state should not exist");
                       return Task.CompletedTask;
                   }
                )
                .Send("set value")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task State_RememberIStoreItemUserState()
        {
            var userState = new UserState(new MemoryStorage());
            var testProperty = userState.CreateProperty<TestPocoState>("test", () => new TestPocoState());
            var adapter = new TestAdapter()
                .Use(userState);

            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var state = await testProperty.GetAsync(context);
                        Assert.IsNotNull(state, "user state should exist");
                        switch ((context.Activity as MessageActivity).Text)
                        {
                            case "set value":
                                state.Value = "test";
                                await context.SendActivityAsync("value saved");
                                break;
                            case "get value":
                                await context.SendActivityAsync(state.Value);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task State_RememberPocoUserState()
        {
            var userState = new UserState(new MemoryStorage());
            var testPocoProperty = userState.CreateProperty<TestPocoState>("testPoco", () => new TestPocoState());
            var adapter = new TestAdapter()
                .Use(userState);
            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var testPocoState = await testPocoProperty.GetAsync(context);
                        Assert.IsNotNull(userState, "user state should exist");
                        switch ((context.Activity as MessageActivity).Text)
                        {
                            case "set value":
                                testPocoState.Value = "test";
                                await context.SendActivityAsync("value saved");
                                break;
                            case "get value":
                                await context.SendActivityAsync(testPocoState.Value);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task State_RememberIStoreItemConversationState()
        {
            var userState = new UserState(new MemoryStorage());
            var testProperty = userState.CreateProperty<TestState>("test", () => new TestState());

            var adapter = new TestAdapter()
                .Use(userState);
            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var conversationState = await testProperty.GetAsync(context);
                        Assert.IsNotNull(conversationState, "state.conversation should exist");
                        switch ((context.Activity as MessageActivity).Text)
                        {
                            case "set value":
                                conversationState.Value = "test";
                                await context.SendActivityAsync("value saved");
                                break;
                            case "get value":
                                await context.SendActivityAsync(conversationState.Value);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task State_RememberPocoConversationState()
        {
            var userState = new UserState(new MemoryStorage());
            var testPocoProperty = userState.CreateProperty<TestPocoState>("testPoco", () => new TestPocoState());
            var adapter = new TestAdapter()
                .Use(userState);
            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var conversationState = await testPocoProperty.GetAsync(context);
                        Assert.IsNotNull(conversationState, "state.conversation should exist");
                        switch ((context.Activity as MessageActivity).Text)
                        {
                            case "set value":
                                conversationState.Value = "test";
                                await context.SendActivityAsync("value saved");
                                break;
                            case "get value":
                                await context.SendActivityAsync(conversationState.Value);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task State_CustomStateManagerTest()
        {

            string testGuid = Guid.NewGuid().ToString();
            var customState = new CustomKeyState(new MemoryStorage());
            var testProperty = customState.CreateProperty<TestPocoState>("test", () => new TestPocoState());

            TestAdapter adapter = new TestAdapter()
                .Use(customState);

            await new TestFlow(adapter, async (context) =>
                    {
                        var test = await testProperty.GetAsync(context);
                        switch ((context.Activity as MessageActivity).Text)
                        {
                            case "set value":
                                test.Value = testGuid;
                                await context.SendActivityAsync("value saved");
                                break;
                            case "get value":
                                await context.SendActivityAsync(test.Value);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", testGuid.ToString())
                .StartTestAsync();
        }

        public class TypedObject
        {
            public string Name { get; set; }
        }

        [TestMethod]
        public async Task State_RoundTripTypedObject()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<TypedObject>("typed", () => new TypedObject());
            var adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var conversation = await testProperty.GetAsync(context);
                        Assert.IsNotNull(conversation, "conversationstate should exist");
                        switch ((context.Activity as MessageActivity).Text)
                        {
                            case "set value":
                                conversation.Name = "test";
                                await context.SendActivityAsync("value saved");
                                break;
                            case "get value":
                                await context.SendActivityAsync(conversation.GetType().Name);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", "TypedObject")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task State_UseBotStateDirectly()
        {
            var adapter = new TestAdapter();

            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var botStateManager = new BotState(new MemoryStorage(),
                            $"BotState:{typeof(BotState).Namespace}.{typeof(BotState).Name}",
                            (ctx) => $"botstate/{ctx.Activity.ChannelId}/{ctx.Activity.Conversation.Id}/{typeof(BotState).Namespace}.{typeof(BotState).Name}");

                        var testProperty = botStateManager.CreateProperty<CustomState>("test", () => new CustomState());

                        // read initial state object
                        await botStateManager.LoadAsync(context);

                        var customState = await testProperty.GetAsync(context);

                        // this should be a 'new CustomState' as nothing is currently stored in storage
                        Assert.Equals(customState, new CustomState());

                        // amend property and write to storage
                        customState.CustomString = "test";
                        await botStateManager.SaveChangesAsync(context);

                        customState.CustomString = "asdfsadf";

                        // read into context again
                        await botStateManager.LoadAsync(context);

                        customState = await testProperty.GetAsync(context);

                        // check object read from value has the correct value for CustomString
                        Assert.Equals(customState.CustomString, "test");
                    }
                )
                .StartTestAsync();
        }

        public class CustomState : IStoreItem
        {
            public string CustomString { get; set; }
            public string ETag { get; set; }
        }

        public class CustomKeyState : BotState
        {
            public CustomKeyState(IStorage storage) : base(storage, PropertyName, (context) => "CustomKey")
            {
            }

            public const string PropertyName = "Microsoft.Bot.Builder.Tests.CustomKeyState";
        }
    }
}
