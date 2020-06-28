// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class BotStateTests
    {
        [Fact]
        public void State_EmptyName()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));

            // Act
            Assert.Throws<ArgumentNullException>(() => userState.CreateProperty<string>(string.Empty));
        }

        [Fact]
        public void State_NullName()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));

            // Act
            Assert.Throws<ArgumentNullException>(() => userState.CreateProperty<string>(null));
        }

        [Fact]
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
            Assert.Equal(0, storeCount);
            await userState.SaveChangesAsync(context);
            await propertyA.SetAsync(context, "hello");
            Assert.Equal(1, readCount);       // Initial save bumps count
            Assert.Equal(0, storeCount);       // Initial save bumps count
            await propertyA.SetAsync(context, "there");
            Assert.Equal(0, storeCount);       // Set on property should not bump
            await userState.SaveChangesAsync(context);
            Assert.Equal(1, storeCount);       // Explicit save should bump
            var valueA = await propertyA.GetAsync(context);
            Assert.Equal("there", valueA);
            Assert.Equal(1, storeCount);       // Gets should not bump
            await userState.SaveChangesAsync(context);
            Assert.Equal(1, storeCount);
            await propertyA.DeleteAsync(context);   // Delete alone no bump
            Assert.Equal(1, storeCount);
            await userState.SaveChangesAsync(context);  // Save when dirty should bump
            Assert.Equal(2, storeCount);
            Assert.Equal(1, readCount);
            await userState.SaveChangesAsync(context);  // Save not dirty should not bump
            Assert.Equal(2, storeCount);
            Assert.Equal(1, readCount);
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public async Task State_GetNoLoadWithDefault()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var propertyA = userState.CreateProperty<string>("propertyA");
            var valueA = await propertyA.GetAsync(context, () => "Default!");
            Assert.Equal("Default!", valueA);
        }

        [Fact]
        public async Task State_GetNoLoadNoDefault()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var propertyA = userState.CreateProperty<string>("propertyA");
            var valueA = await propertyA.GetAsync(context);

            // Assert
            Assert.Null(valueA);
        }

        [Fact]
        public async Task State_POCO_NoDefault()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var testProperty = userState.CreateProperty<TestPocoState>("test");
            var value = await testProperty.GetAsync(context);

            // Assert
            Assert.Null(value);
        }

        [Fact]
        public async Task State_bool_NoDefault()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var testProperty = userState.CreateProperty<bool>("test");
            var value = await testProperty.GetAsync(context);

            // Assert
            Assert.False(value);
        }

        [Fact]
        public async Task State_int_NoDefault()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var testProperty = userState.CreateProperty<int>("test");
            var value = await testProperty.GetAsync(context);

            // Assert
            Assert.Equal(0, value);
        }

        [Fact]
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

        [Fact]
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
            Assert.Equal("hello2", valueA);
        }

        [Fact]
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
            var obj = dictionary["EmptyContext/users/empty@empty.context.org"];
            Assert.Equal("hello", obj["property-a"]);
            Assert.Equal("world", obj["property-b"]);
        }

        [Fact]
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
            var obj = dictionary["EmptyContext/users/empty@empty.context.org"];
            Assert.Equal("hello", obj["property-a"]);
            Assert.Equal("world", obj["property-b"]);

            // Act 2
            var userState2 = new UserState(new MemoryStorage(dictionary));

            var propertyA2 = userState2.CreateProperty<string>("property-a");
            var propertyB2 = userState2.CreateProperty<string>("property-b");

            await userState2.LoadAsync(context);
            await propertyA.SetAsync(context, "hello-2");
            await propertyB.SetAsync(context, "world-2");
            await userState2.SaveChangesAsync(context);

            // Assert 2
            var obj2 = dictionary["EmptyContext/users/empty@empty.context.org"];
            Assert.Equal("hello-2", obj2["property-a"]);
            Assert.Equal("world-2", obj2["property-b"]);
            Assert.Equal("test", obj2["property-c"]);
        }

        [Fact]
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
            var obj = dictionary["EmptyContext/users/empty@empty.context.org"];
            Assert.Equal("hello", obj["property-a"]);
            Assert.Equal("world", obj["property-b"]);

            // Act 2
            var userState2 = new UserState(new MemoryStorage(dictionary));

            var propertyA2 = userState2.CreateProperty<string>("property-a");
            var propertyB2 = userState2.CreateProperty<string>("property-b");

            await userState2.LoadAsync(context);
            await propertyA.SetAsync(context, "hello-2");
            await propertyB.DeleteAsync(context);
            await userState2.SaveChangesAsync(context);

            // Assert 2
            var obj2 = dictionary["EmptyContext/users/empty@empty.context.org"];
            Assert.Equal("hello-2", obj2["property-a"]);
            Assert.Null(obj2["property-b"]);
        }

        [Fact]
        public async Task State_DoNOTRememberContextState()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("State_DoNOTRememberContextState"));

            await new TestFlow(adapter, (context, cancellationToken) =>
            {
                var obj = context.TurnState.Get<UserState>();
                Assert.Null(obj);
                return Task.CompletedTask;
            })
            .Send("set value")
            .StartTestAsync();
        }

        [Fact]
        public async Task State_RememberIStoreItemUserState()
        {
            var userState = new UserState(new MemoryStorage());
            var testProperty = userState.CreateProperty<TestPocoState>("test");
            var adapter = new TestAdapter(TestAdapter.CreateConversation("State_RememberIStoreItemUserState"))
                .Use(new AutoSaveStateMiddleware(userState));

            await new TestFlow(
                adapter,
                async (context, cancellationToken) =>
                {
                    var state = await testProperty.GetAsync(context, () => new TestPocoState());
                    Assert.NotNull(state);
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
                })
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTestAsync();
        }

        [Fact]
        public async Task State_RememberPocoUserState()
        {
            var userState = new UserState(new MemoryStorage());
            var testPocoProperty = userState.CreateProperty<TestPocoState>("testPoco");
            var adapter = new TestAdapter(TestAdapter.CreateConversation("tate_RememberPocoUserState"))
                .Use(new AutoSaveStateMiddleware(userState));
            await new TestFlow(
                adapter,
                async (context, cancellationToken) =>
                    {
                        var testPocoState = await testPocoProperty.GetAsync(context, () => new TestPocoState());
                        Assert.NotNull(userState);
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
                    })
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTestAsync();
        }

        [Fact]
        public async Task State_RememberIStoreItemConversationState()
        {
            var userState = new UserState(new MemoryStorage());
            var testProperty = userState.CreateProperty<TestState>("test");

            var adapter = new TestAdapter(TestAdapter.CreateConversation("State_RememberIStoreItemConversationState"))
                .Use(new AutoSaveStateMiddleware(userState));

            await new TestFlow(
                adapter,
                async (context, cancellationToken) =>
                    {
                        var conversationState = await testProperty.GetAsync(context, () => new TestState());
                        Assert.NotNull(conversationState);
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
                    })
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTestAsync();
        }

        [Fact]
        public async Task State_RememberPocoConversationState()
        {
            var userState = new UserState(new MemoryStorage());
            var testPocoProperty = userState.CreateProperty<TestPocoState>("testPoco");
            var adapter = new TestAdapter(TestAdapter.CreateConversation("State_RememberPocoConversationState"))
                .Use(new AutoSaveStateMiddleware(userState));

            await new TestFlow(
                adapter,
                async (context, cancellationToken) =>
                    {
                        var conversationState = await testPocoProperty.GetAsync(context, () => new TestPocoState());
                        Assert.NotNull(conversationState);
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
                    })
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTestAsync();
        }

        [Fact]
        public async Task State_RememberPocoPrivateConversationState()
        {
            var privateConversationState = new PrivateConversationState(new MemoryStorage());
            var testPocoProperty = privateConversationState.CreateProperty<TestPocoState>("testPoco");
            var adapter = new TestAdapter(TestAdapter.CreateConversation("State_RememberPocoPrivateConversationState"))
                .Use(new AutoSaveStateMiddleware(privateConversationState));

            await new TestFlow(
                adapter,
                async (context, cancellationToken) =>
                    {
                        var conversationState = await testPocoProperty.GetAsync(context, () => new TestPocoState());
                        Assert.NotNull(conversationState);
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
                    })
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTestAsync();
        }

        [Fact]
        public async Task State_CustomStateManagerTest()
        {
            var testGuid = Guid.NewGuid().ToString();
            var customState = new CustomKeyState(new MemoryStorage());

            var testProperty = customState.CreateProperty<TestPocoState>("test");

            var adapter = new TestAdapter(TestAdapter.CreateConversation("State_CustomStateManagerTest"))
                .Use(new AutoSaveStateMiddleware(customState));

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
                    })
                .Test("set value", "value saved")
                .Test("get value", testGuid.ToString())
                .StartTestAsync();
        }

        [Fact]
        public async Task State_RoundTripTypedObject()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<TypedObject>("typed");
            var adapter = new TestAdapter(TestAdapter.CreateConversation("State_RoundTripTypedObject"))
                .Use(new AutoSaveStateMiddleware(convoState));

            await new TestFlow(
                adapter,
                async (context, cancellationToken) =>
                    {
                        var conversation = await testProperty.GetAsync(context, () => new TypedObject());
                        Assert.NotNull(conversation);
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
                    })
                .Test("set value", "value saved")
                .Test("get value", "TypedObject")
                .StartTestAsync();
        }

        [Fact]
        public async Task State_UseBotStateDirectly()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("State_UseBotStateDirectly"));

            await new TestFlow(
                adapter,
                async (context, cancellationToken) =>
                    {
                        var botStateManager = new TestBotState(new MemoryStorage());

                        var testProperty = botStateManager.CreateProperty<CustomState>("test");

                        // read initial state object
                        await botStateManager.LoadAsync(context);

                        var customState = await testProperty.GetAsync(context, () => new CustomState());

                        // this should be a 'new CustomState' as nothing is currently stored in storage
                        Assert.NotNull(customState);
                        Assert.IsType<CustomState>(customState);
                        Assert.Null(customState.CustomString);

                        // amend property and write to storage
                        customState.CustomString = "test";
                        await botStateManager.SaveChangesAsync(context);

                        customState.CustomString = "asdfsadf";

                        // read into context again
                        await botStateManager.LoadAsync(context, force: true);

                        customState = await testProperty.GetAsync(context);

                        // check object read from value has the correct value for CustomString
                        Assert.Equal("test", customState.CustomString);
                    })
                .Send(new Activity() { Type = ActivityTypes.ConversationUpdate })
                .StartTestAsync();
        }

        [Fact]
        public async Task UserState_BadFromThrows()
        {
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();
            context.Activity.From = null;
            var testProperty = userState.CreateProperty<TestPocoState>("test");
            await Assert.ThrowsAsync<InvalidOperationException>(() => testProperty.GetAsync(context));
        }

        [Fact]
        public async Task ConversationState_BadConverationThrows()
        {
            var dictionary = new Dictionary<string, JObject>();
            var userState = new ConversationState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();
            context.Activity.Conversation = null;
            var testProperty = userState.CreateProperty<TestPocoState>("test");
            await Assert.ThrowsAsync<InvalidOperationException>(() => testProperty.GetAsync(context));
        }

        [Fact]
        public async Task PrivateConversationState_BadActivityFromThrows()
        {
            var dictionary = new Dictionary<string, JObject>();
            var userState = new PrivateConversationState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();
            context.Activity.Conversation = null;
            context.Activity.From = null;
            var testProperty = userState.CreateProperty<TestPocoState>("test");
            await Assert.ThrowsAsync<InvalidOperationException>(() => testProperty.GetAsync(context));
        }

        [Fact]
        public async Task PrivateConversationState_BadActivityConversationThrows()
        {
            var dictionary = new Dictionary<string, JObject>();
            var userState = new PrivateConversationState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();
            context.Activity.Conversation = null;
            var testProperty = userState.CreateProperty<TestPocoState>("test");
            await Assert.ThrowsAsync<InvalidOperationException>(() => testProperty.GetAsync(context));
        }

        [Fact]
        public async Task ClearAndSave()
        {
            var turnContext = TestUtilities.CreateEmptyContext();
            turnContext.Activity.Conversation = new ConversationAccount { Id = "1234" };

            var storage = new MemoryStorage(new Dictionary<string, JObject>());

            // Turn 0
            var botState1 = new ConversationState(storage);
            (await botState1
                .CreateProperty<TestPocoState>("test-name")
                .GetAsync(turnContext, () => new TestPocoState())).Value = "test-value";
            await botState1.SaveChangesAsync(turnContext);

            // Turn 1
            var botState2 = new ConversationState(storage);
            var value1 = (await botState2
                .CreateProperty<TestPocoState>("test-name")
                .GetAsync(turnContext, () => new TestPocoState { Value = "default-value" })).Value;

            Assert.Equal("test-value", value1);

            // Turn 2
            var botState3 = new ConversationState(storage);
            await botState3.ClearStateAsync(turnContext);
            await botState3.SaveChangesAsync(turnContext);

            // Turn 3
            var botState4 = new ConversationState(storage);
            var value2 = (await botState4
                .CreateProperty<TestPocoState>("test-name")
                .GetAsync(turnContext, () => new TestPocoState { Value = "default-value" })).Value;

            Assert.Equal("default-value", value2);
        }

        [Fact]
        public async Task BotStateDelete()
        {
            var turnContext = TestUtilities.CreateEmptyContext();
            turnContext.Activity.Conversation = new ConversationAccount { Id = "1234" };

            var storage = new MemoryStorage(new Dictionary<string, JObject>());

            // Turn 0
            var botState1 = new ConversationState(storage);
            (await botState1
                .CreateProperty<TestPocoState>("test-name")
                .GetAsync(turnContext, () => new TestPocoState())).Value = "test-value";
            await botState1.SaveChangesAsync(turnContext);

            // Turn 1
            var botState2 = new ConversationState(storage);
            var value1 = (await botState2
                .CreateProperty<TestPocoState>("test-name")
                .GetAsync(turnContext, () => new TestPocoState { Value = "default-value" })).Value;

            Assert.Equal("test-value", value1);

            // Turn 2
            var botState3 = new ConversationState(storage);
            await botState3.DeleteAsync(turnContext);

            // Turn 3
            var botState4 = new ConversationState(storage);
            var value2 = (await botState4
                .CreateProperty<TestPocoState>("test-name")
                .GetAsync(turnContext, () => new TestPocoState { Value = "default-value" })).Value;

            Assert.Equal("default-value", value2);
        }

        [Fact]
        public async Task BotStateGet()
        {
            var turnContext = TestUtilities.CreateEmptyContext();
            turnContext.Activity.Conversation = new ConversationAccount { Id = "1234" };

            var storage = new MemoryStorage(new Dictionary<string, JObject>());

            // This was changed from ConversationSate to TestBotState
            // because TestBotState has a context service key
            // that is different from the name of its type
            var botState = new TestBotState(storage);
            (await botState
                .CreateProperty<TestPocoState>("test-name")
                .GetAsync(turnContext, () => new TestPocoState())).Value = "test-value";

            var json = botState.Get(turnContext);

            Assert.Equal("test-value", json["test-name"]["Value"].ToString());
        }

        [Fact]
        public async Task BotStateGetCachedState()
        {
            var turnContext = TestUtilities.CreateEmptyContext();
            turnContext.Activity.Conversation = new ConversationAccount { Id = "1234" };

            var storage = new MemoryStorage(new Dictionary<string, JObject>());
            var botState = new TestBotState(storage);

            (await botState
                .CreateProperty<TestPocoState>("test-name")
                .GetAsync(turnContext, () => new TestPocoState())).Value = "test-value";

            var cache = botState.GetCachedState(turnContext);

            Assert.NotNull(cache);

            Assert.Same(cache, botState.GetCachedState(turnContext));
        }

        [Fact]
        public async Task State_ForceIsNoOpWithoutCachedBotState()
        {
            // Arrange
            var dictionary = new Dictionary<string, JObject>();
            var userState = new UserState(new MemoryStorage(dictionary));
            var context = TestUtilities.CreateEmptyContext();

            // Act
            await userState.SaveChangesAsync(context, true);
        }

        [Fact]
        public async Task State_ForceCallsSaveWithoutCachedBotStateChanges()
        {
            // Mock a storage provider, which counts writes
            var storeCount = 0;
            var dictionary = new Dictionary<string, object>();
            var mock = new Mock<IStorage>();
            mock.Setup(ms => ms.WriteAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => storeCount++);
            mock.Setup(ms => ms.ReadAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(result: (IDictionary<string, object>)dictionary));

            // Arrange
            var userState = new UserState(mock.Object);
            var context = TestUtilities.CreateEmptyContext();

            // Act
            var propertyA = userState.CreateProperty<string>("propertyA");

            // Set initial value and save
            await propertyA.SetAsync(context, "test");
            await userState.SaveChangesAsync(context);

            // Assert
            Assert.Equal(1, storeCount);

            // Saving without changes and wthout force does NOT call .WriteAsync
            await userState.SaveChangesAsync(context);
            Assert.Equal(1, storeCount);

            // Forcing save without changes DOES call .WriteAsync
            await userState.SaveChangesAsync(context, true);
            Assert.Equal(2, storeCount);
        }

        public class TypedObject
        {
            public string Name { get; set; }
        }

        public class TestBotState : BotState
        {
            public TestBotState(IStorage storage)
                : base(storage, $"BotState:{typeof(BotState).Namespace}.{typeof(BotState).Name}")
            {
            }

            protected override string GetStorageKey(ITurnContext turnContext) => $"botstate/{turnContext.Activity.ChannelId}/{turnContext.Activity.Conversation.Id}/{typeof(BotState).Namespace}.{typeof(BotState).Name}";
        }

        public class CustomState : IStoreItem
        {
            public string CustomString { get; set; }

            public string ETag { get; set; }
        }

        public class CustomKeyState : BotState
        {
            public const string PropertyName = "Microsoft.Bot.Builder.Tests.CustomKeyState";

            public CustomKeyState(IStorage storage)
                : base(storage, PropertyName)
            {
            }

            protected override string GetStorageKey(ITurnContext turnContext) => "CustomKey";
        }
    }
}
