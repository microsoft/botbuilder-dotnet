// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    public class TestState : IStoreItem
    {
        public string ETag { get ; set; }
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
                       var obj = context.GetConversationState<TestPocoState>();
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
            var adapter = new TestAdapter()
                .Use(new UserState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var userState = context.GetUserState<TestState>();
                        Assert.IsNotNull(userState, "user state should exist");
                        switch (context.Activity.Text)
                        {
                            case "set value":
                                userState.Value = "test";
                                await context.SendActivityAsync("value saved");
                                break;
                            case "get value":
                                await context.SendActivityAsync(userState.Value);
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
            var adapter = new TestAdapter()
                .Use(new UserState<TestPocoState>(new MemoryStorage()));
            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var userState = context.GetUserState<TestPocoState>();
                        Assert.IsNotNull(userState, "user state should exist");
                        switch (context.Activity.AsMessageActivity().Text)
                        {
                            case "set value":
                                userState.Value = "test";
                                await context.SendActivityAsync("value saved");
                                break;
                            case "get value":
                                await context.SendActivityAsync(userState.Value);
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
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));
            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var conversationState = context.GetConversationState<TestState>();
                        Assert.IsNotNull(conversationState, "state.conversation should exist");
                        switch (context.Activity.AsMessageActivity().Text)
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
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestPocoState>(new MemoryStorage()));
            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var conversationState = context.GetConversationState<TestPocoState>();
                        Assert.IsNotNull(conversationState, "state.conversation should exist");
                        switch (context.Activity.AsMessageActivity().Text)
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
            TestAdapter adapter = new TestAdapter()
                .Use(new CustomKeyState(new MemoryStorage()));
            await new TestFlow(adapter, async (context) =>
                    {
                        var customState = CustomKeyState.Get(context);
                        switch (context.Activity.AsMessageActivity().Text)
                        {
                            case "set value":
                                customState.CustomString = testGuid;
                                await context.SendActivityAsync("value saved");
                                break;
                            case "get value":
                                await context.SendActivityAsync(customState.CustomString);
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
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TypedObject>(new MemoryStorage()));

            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var conversation = context.GetConversationState<TypedObject>();
                        Assert.IsNotNull(conversation, "conversationstate should exist");
                        switch (context.Activity.AsMessageActivity().Text)
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
                        var botStateManager = new BotState<CustomState>(new MemoryStorage(),
                            $"BotState:{typeof(BotState<CustomState>).Namespace}.{typeof(BotState<CustomState>).Name}",
                            (ctx) => $"botstate/{ctx.Activity.ChannelId}/{ctx.Activity.Conversation.Id}/{typeof(BotState<CustomState>).Namespace}.{typeof(BotState<CustomState>).Name}");

                        // read initial state object
                        var customState = await botStateManager.ReadAsync(context);

                        // this should be a 'new CustomState' as nothing is currently stored in storage
                        Assert.Equals(customState, new CustomState());

                        // amend property and write to storage
                        customState.CustomString = "test";
                        await botStateManager.WriteAsync(context, customState);

                        // set customState to null before reading from storage
                        customState = null;
                        customState = await botStateManager.ReadAsync(context);

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

        public class CustomKeyState : BotState<CustomState>
        {
            public CustomKeyState(IStorage storage) : base(storage, PropertyName, (context) => "CustomKey")
            {
            }

            public const string PropertyName = "Microsoft.Bot.Builder.Tests.CustomKeyState";

            public static CustomState Get(ITurnContext context) { return context.Services.Get<CustomState>(PropertyName); }
        }
    }
}
