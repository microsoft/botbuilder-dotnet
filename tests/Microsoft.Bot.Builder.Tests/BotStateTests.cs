// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Tests
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
        public async Task LoadSetSave()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var propertyA = userState.CreateProperty<string>("property-a");
            var propertyB = userState.CreateProperty<string>("property-b");

            await userState.LoadAsync(context);
            await propertyA.SetAsync(context, "hello");
            await propertyB.SetAsync(context, "world");
            await userState.SaveChangesAsync(context);

            // Assert
            var obj = dictionary["user/EmptyContext/empty@empty.context.org"];
            Assert.AreEqual("hello", obj["property-a"]);
            Assert.AreEqual("world", obj["property-b"]);
        }

        [TestMethod]
        public async Task LoadSetSaveTwice()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var userState = new UserState(new MemoryStorage(dictionary));

            var propertyA = userState.CreateProperty<string>("property-a");
            var propertyB = userState.CreateProperty<string>("property-b");
            var propertyC = userState.CreateProperty<string>("property-c");

            await userState.LoadAsync(context);
            await propertyA.SetAsync(context, "hello");
            await propertyB.SetAsync(context, "world");
            await propertyC.SetAsync(context, "test");
            await userState.SaveChangesAsync(context);

            // Assert
            var obj = dictionary["user/EmptyContext/empty@empty.context.org"];
            Assert.AreEqual("hello", obj["property-a"]);
            Assert.AreEqual("world", obj["property-b"]);

            // Act 2
            var userState2 = new UserState(new MemoryStorage(dictionary));

            var propertyA2 = userState2.CreateProperty<string>("property-a");
            var propertyB2 = userState2.CreateProperty<string>("property-b");

            await userState2.LoadAsync(context);
            await propertyA.SetAsync(context, "hello-2");
            await propertyB.SetAsync(context, "world-2");
            await userState2.SaveChangesAsync(context);

            // Assert 2
            var obj2 = dictionary["user/EmptyContext/empty@empty.context.org"];
            Assert.AreEqual("hello-2", obj2["property-a"]);
            Assert.AreEqual("world-2", obj2["property-b"]);
            Assert.AreEqual("test", obj2["property-c"]);
        }

        [TestMethod]
        public async Task LoadSaveDelete()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var userState = new UserState(new MemoryStorage(dictionary));

            var propertyA = userState.CreateProperty<string>("property-a");
            var propertyB = userState.CreateProperty<string>("property-b");

            await userState.LoadAsync(context);
            await propertyA.SetAsync(context, "hello");
            await propertyB.SetAsync(context, "world");
            await userState.SaveChangesAsync(context);

            // Assert
            var obj = dictionary["user/EmptyContext/empty@empty.context.org"];
            Assert.AreEqual("hello", obj["property-a"]);
            Assert.AreEqual("world", obj["property-b"]);

            // Act 2
            var userState2 = new UserState(new MemoryStorage(dictionary));

            var propertyA2 = userState2.CreateProperty<string>("property-a");
            var propertyB2 = userState2.CreateProperty<string>("property-b");

            await userState2.LoadAsync(context);
            await propertyA.SetAsync(context, "hello-2");
            await propertyB.DeleteAsync(context);
            await userState2.SaveChangesAsync(context);

            // Assert 2
            var obj2 = dictionary["user/EmptyContext/empty@empty.context.org"];
            Assert.AreEqual("hello-2", obj2["property-a"]);
            Assert.IsNull(obj2["property-b"]);
        }

        [TestMethod]
        public async Task State_DoNOTRememberContextState()
        {

            TestAdapter adapter = new TestAdapter();

            await new TestFlow(adapter, (context, cancellationToken) =>
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
            var testProperty = userState.CreateProperty("test", () => new TestPocoState());
            var adapter = new TestAdapter()
                .Use(userState);

            await new TestFlow(adapter,
                    async (context, cancellationToken) =>
                    {
                        var state = await testProperty.GetAsync(context);
                        Assert.IsNotNull(state, "user state should exist");
                        switch (context.Activity.Text)
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
            var testPocoProperty = userState.CreateProperty("testPoco", () => new TestPocoState());
            var adapter = new TestAdapter()
                .Use(userState);
            await new TestFlow(adapter,
                    async (context, cancellationToken) =>
                    {
                        var testPocoState = await testPocoProperty.GetAsync(context);
                        Assert.IsNotNull(userState, "user state should exist");
                        switch (context.Activity.AsMessageActivity().Text)
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
            var testProperty = userState.CreateProperty("test", () => new TestState());

            var adapter = new TestAdapter()
                .Use(userState);
            await new TestFlow(adapter,
                    async (context, cancellationToken) =>
                    {
                        var conversationState = await testProperty.GetAsync(context);
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
            var userState = new UserState(new MemoryStorage());
            var testPocoProperty = userState.CreateProperty("testPoco", () => new TestPocoState());
            var adapter = new TestAdapter()
                .Use(userState);
            await new TestFlow(adapter,
                    async (context, cancellationToken) =>
                    {
                        var conversationState = await testPocoProperty.GetAsync(context);
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
            var customState = new CustomKeyState(new MemoryStorage());
            var testProperty = customState.CreateProperty("test", () => new TestPocoState());

            TestAdapter adapter = new TestAdapter()
                .Use(customState);

            await new TestFlow(adapter, async (context, cancellationToken) =>
                    {
                        var test = await testProperty.GetAsync(context);
                        switch (context.Activity.AsMessageActivity().Text)
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
            var testProperty = convoState.CreateProperty("typed", () => new TypedObject());
            var adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter,
                    async (context, cancellationToken) =>
                    {
                        var conversation = await testProperty.GetAsync(context);
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
                    async (context, cancellationToken) =>
                    {
                    var botStateManager = new TestBotState(new MemoryStorage());

                        var testProperty = botStateManager.CreateProperty("test", () => new CustomState());

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

        public class TestBotState : BotState
        {
            public TestBotState(IStorage storage)
                : base(storage, $"BotState:{typeof(BotState).Namespace}.{typeof(BotState).Name}")
            {
            }

            protected override string GetStorageKey(ITurnContext context) => $"botstate/{context.Activity.ChannelId}/{context.Activity.Conversation.Id}/{typeof(BotState).Namespace}.{typeof(BotState).Name}";
        }

        public class CustomState : IStoreItem
        {
            public string CustomString { get; set; }
            public string ETag { get; set; }
        }

        public class CustomKeyState : BotState
        {
            public CustomKeyState(IStorage storage) : base(storage, PropertyName)
            {
            }

            public const string PropertyName = "Microsoft.Bot.Builder.Tests.CustomKeyState";

            protected override string GetStorageKey(ITurnContext context) => "CustomKey";
        }
    }
}
