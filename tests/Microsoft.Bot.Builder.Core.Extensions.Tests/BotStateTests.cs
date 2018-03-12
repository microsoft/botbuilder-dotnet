// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    public class TestState : IStoreItem
    {
        public string eTag { get ; set; }
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

            await new TestFlow(adapter, async (context) =>
                   {
                       var obj = context.GetConversationState<StoreItem>();
                       Assert.IsNull(obj, "context.state should not exist");
                   }
                )
                .Send("set value")
                .StartTest();
        }

        [TestMethod]
        public async Task State_RememberIStoreItemUserState()
        {
            var adapter = new TestAdapter()
                .Use(new BatchOutputMiddleware())
                .Use(new UserState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var userState = context.GetUserState<TestState>();
                        Assert.IsNotNull(userState, "user state should exist");
                        switch (context.Request.Text)
                        {
                            case "set value":
                                userState.Value = "test";
                                context.Batch().Reply("value saved");
                                break;
                            case "get value":
                                context.Batch().Reply(userState.Value);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTest();
        }

        [TestMethod]
        public async Task State_RememberPocoUserState()
        {
            var adapter = new TestAdapter()
                .Use(new UserState<TestPocoState>(new MemoryStorage()));
            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var userState = context.GetUserState<TestPocoState>();
                        Assert.IsNotNull(userState, "user state should exist");
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                userState.Value = "test";
                                await context.SendActivity("value saved");
                                break;
                            case "get value":
                                await context.SendActivity(userState.Value);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTest();
        }

        [TestMethod]
        public async Task State_RememberIStoreItemConversationState()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new BatchOutputMiddleware())
                .Use(new ConversationState<TestState>(new MemoryStorage()));
            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var conversationState = context.GetConversationState<TestState>();
                        Assert.IsNotNull(conversationState, "state.conversation should exist");
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                conversationState.Value = "test";
                                context.Batch().Reply("value saved");
                                break;
                            case "get value":
                                context.Batch().Reply(conversationState.Value);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTest();
        }

        [TestMethod]
        public async Task State_RememberPocoConversationState()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestPocoState>(new MemoryStorage()));
            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var conversationState = context.GetConversationState<TestPocoState>();
                        Assert.IsNotNull(conversationState, "state.conversation should exist");
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                conversationState.Value = "test";
                                await context.SendActivity("value saved");
                                break;
                            case "get value":
                                await context.SendActivity(conversationState.Value);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTest();
        }

        [TestMethod]
        public async Task State_CustomStateManagerTest()
        {

            string testGuid = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter()
                .Use(new BatchOutputMiddleware())
                .Use(new CustomKeyState(new MemoryStorage()));
            await new TestFlow(adapter, async (context) =>
                    {
                        var customState = CustomKeyState.Get(context);
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                customState.CustomString = testGuid;
                                context.Batch().Reply("value saved");
                                break;
                            case "get value":
                                context.Batch().Reply(customState.CustomString);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", testGuid.ToString())
                .StartTest();
        }

        public class TypedObject : StoreItem
        {
            public string Name { get; set; }
        }

        [TestMethod]
        public async Task State_RoundTripTypedObject()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new BatchOutputMiddleware())
                .Use(new ConversationState<TypedObject>(new MemoryStorage()));

            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var conversation = context.GetConversationState<TypedObject>();
                        Assert.IsNotNull(conversation, "conversationstate should exist");
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                conversation.Name = "test";
                                context.Batch().Reply("value saved");
                                break;
                            case "get value":
                                context.Batch().Reply(conversation.GetType().Name);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", "TypedObject")
                .StartTest();
        }

        public class CustomState : StoreItem
        {
            public string CustomString { get; set; }
        }

        public class CustomKeyState : BotState<CustomState>
        {
            public CustomKeyState(IStorage storage) : base(storage, PropertyName, (context) => "CustomKey")
            {
            }

            public const string PropertyName = "Microsoft.Bot.Builder.Tests.CustomKeyState";

            public static CustomState Get(IBotContext context) { return context.Get<CustomState>(PropertyName); }
        }
    }
}
