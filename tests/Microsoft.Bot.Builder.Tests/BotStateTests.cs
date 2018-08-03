// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
        [ExpectedException(typeof(ArgumentNullException), "Cannot have empty/null property name")]
        public void State_EmptyName()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));

            // Act
            var propertyA = userState.CreateProperty<string>("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Cannot have empty/null property name")]
        public void State_NullName()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));

            // Act
            var propertyA = userState.CreateProperty<string>(null);
        }

        [TestMethod,
         Description("Verify storage not called when no changes are made")]
        public async Task MakeSureStorageNotCalledNoChangesAsync()
        {
            // Mock a storage provider, which counts read/writes
            var storeCount = 0;
            var readCount = 0;
            var dictionary = new Dictionary<string, object>();
            var mock = new Mock<IStorage>();
            mock.Setup(ms => ms.WriteAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => storeCount++);
            mock.Setup(ms => ms.ReadAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(result: (IDictionary<string, object>)dictionary))
                .Callback(() => readCount++);


            // Arrange
            var userState = new UserState(mock.Object);
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var propertyA = userState.CreateProperty<string>("propertyA");
            Assert.AreEqual(storeCount, 0);
            await userState.SaveChangesAsync(context);
            await propertyA.SetAsync(context, "hello");
            Assert.AreEqual(readCount, 1);       // Initial save bumps count
            Assert.AreEqual(storeCount, 0);       // Initial save bumps count
            await propertyA.SetAsync(context, "there");
            Assert.AreEqual(storeCount, 0);       // Set on property should not bump
            await userState.SaveChangesAsync(context);
            Assert.AreEqual(storeCount, 1);       // Explicit save should bump
            var valueA = await propertyA.GetAsync(context);
            Assert.AreEqual("there", valueA);
            Assert.AreEqual(storeCount, 1);       // Gets should not bump
            await userState.SaveChangesAsync(context);
            Assert.AreEqual(storeCount, 1);
            await propertyA.DeleteAsync(context);   // Delete alone no bump
            Assert.AreEqual(storeCount, 1);
            await userState.SaveChangesAsync(context);  // Save when dirty should bump
            Assert.AreEqual(storeCount, 2);
            Assert.AreEqual(readCount, 1);
            await userState.SaveChangesAsync(context);  // Save not dirty should not bump
            Assert.AreEqual(storeCount, 2);
            Assert.AreEqual(readCount, 1);

        }



        [TestMethod, 
         Description("Should be able to set a property with no Load")]
        public async Task State_SetNoLoad()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var propertyA = userState.CreateProperty<string>("propertyA");
            await propertyA.SetAsync(context, "hello");
        }


        [TestMethod,
         Description("Should be able to load multiple times")]
        public async Task State_MultipleLoads()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var propertyA = userState.CreateProperty<string>("propertyA");
            await userState.LoadAsync(context);
            await userState.LoadAsync(context);

        }
        [TestMethod,
         Description("Should be able to get a property with no Load and default")]
        public async Task State_GetNoLoadWithDefault()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var propertyA = userState.CreateProperty<string>("propertyA");
            var valueA = await propertyA.GetAsync(context, () => "Default!");
            Assert.AreEqual("Default!", valueA);
        }


        [TestMethod,
         Description("Cannot get a string with no default set")]
        [ExpectedException(typeof(MissingMemberException), "Get on unset member should throw MissingMemberException")]
        public async Task State_GetNoLoadNoDefault()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var propertyA = userState.CreateProperty<string>("propertyA");
            var valueA = await propertyA.GetAsync(context);
        }

        [TestMethod,
         Description("Cannot get a POCO with no default set")]
        [ExpectedException(typeof(MissingMemberException), "Get on unset member should throw MissingMemberException")]
        public async Task State_POCO_NoDefault()
        {
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            var testProperty = userState.CreateProperty<TestPocoState>("test");

            var value = await testProperty.GetAsync(context);
        }


        [TestMethod,
         Description("Cannot get a bool with no default set")]
        [ExpectedException(typeof(MissingMemberException), "Get on unset member should throw MissingMemberException")]
        public async Task State_bool_NoDefault()
        {
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            var testProperty = userState.CreateProperty<bool>("test");

            var value = await testProperty.GetAsync(context);
            Assert.IsFalse(value);

        }

        [TestMethod,
         Description("Cannot get a int with no default set")]
        [ExpectedException(typeof(MissingMemberException), "Get on unset member should throw MissingMemberException")]
        public async Task State_int_NoDefault()
        {
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            var testProperty = userState.CreateProperty<int>("test");
            var value = await testProperty.GetAsync(context);

        }




        [TestMethod,
         Description("Verify setting property after save")]
        public async Task State_SetAfterSave()
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

            await propertyA.SetAsync(context, "hello2");

        }

        [TestMethod, 
            Description("Verify multiple saves")]
        public async Task State_MultipleSave()
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

            await propertyA.SetAsync(context, "hello2");
            await userState.SaveChangesAsync(context);
            var valueA = await propertyA.GetAsync(context);
            Assert.AreEqual("hello2", valueA);

        }

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

            var adapter = new TestAdapter();

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
            var testProperty = userState.CreateProperty<TestPocoState>("test");
            var adapter = new TestAdapter()
                .Use(userState);

            await new TestFlow(adapter,
                    async (context, cancellationToken) =>
                    {
                        var state = await testProperty.GetAsync(context, () => new TestPocoState());
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
            var testPocoProperty = userState.CreateProperty<TestPocoState>("testPoco");
            var adapter = new TestAdapter()
                .Use(userState);
            await new TestFlow(adapter,
                    async (context, cancellationToken) =>
                    {
                        var testPocoState = await testPocoProperty.GetAsync(context, () => new TestPocoState());
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
            var testProperty = userState.CreateProperty<TestState>("test");

            var adapter = new TestAdapter()
                .Use(userState);
            await new TestFlow(adapter,
                    async (context, cancellationToken) =>
                    {
                        var conversationState = await testProperty.GetAsync(context, () => new TestState());
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
            var testPocoProperty = userState.CreateProperty<TestPocoState>("testPoco");
            var adapter = new TestAdapter()
                .Use(userState);
            await new TestFlow(adapter,
                    async (context, cancellationToken) =>
                    {
                        var conversationState = await testPocoProperty.GetAsync(context, () => new TestPocoState());
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

            var testGuid = Guid.NewGuid().ToString();
            var customState = new CustomKeyState(new MemoryStorage());

            var testProperty = customState.CreateProperty<TestPocoState>("test");

            var adapter = new TestAdapter()
                .Use(customState);

            await new TestFlow(adapter, async (context, cancellationToken) =>
                    {
                        var test = await testProperty.GetAsync(context, () => new TestPocoState());
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
            var testProperty = convoState.CreateProperty<TypedObject>("typed");
            var adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter,
                    async (context, cancellationToken) =>
                    {
                        var conversation = await testProperty.GetAsync(context, () => new TypedObject());
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

                        var testProperty = botStateManager.CreateProperty<CustomState>("test");

                        // read initial state object
                        await botStateManager.LoadAsync(context);

                        var customState = await testProperty.GetAsync(context, () => new CustomState());

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
