// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    public class TestState
    {
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
        public async Task State_ThrowIfNoConfiguredStateStorageProvider()
        {
            TestAdapter adapter = new TestAdapter();

            await new TestFlow(adapter, async (context) =>
                   {
                       try
                       {
                           await context.ConversationState().Get<TestState>();
                       }
                       catch(InvalidOperationException exception)
                       {
                           StringAssert.Contains(exception.Message, nameof(IStateManagerServiceResolver));
                           StringAssert.Contains(exception.Message, nameof(StateManagementMiddleware));
                       }
                   }
                )
                .Send("set value")
                .StartTest();
        }

        [TestMethod]
        public async Task State_UnsetStateShouldReturnNull()
        {
            var adapter = new TestAdapter()
                .Use(new StateManagementMiddleware()
                        .UseDefaultStorageProvider(new MemoryStateStorageProvider())
                        .UseUserState());

            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var testState = await context.UserState().Get<TestState>();

                        Assert.IsNull(testState, "test state should not exist yet as it hasn't yet been created");
                    }
                )
                .Send("test message")
                .StartTest();
        }

        [TestMethod]
        public async Task State_UnsavedStateShouldNotPersist()
        {
            var adapter = new TestAdapter()
                .Use(new StateManagementMiddleware()
                        .UseDefaultStorageProvider(new MemoryStateStorageProvider())
                        .UseUserState());

            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var userState = context.UserState();

                        Assert.IsNotNull(userState);

                        var testState = await userState.Get<TestState>();

                        switch (context.Activity.Text)
                        {
                            case "don't save on this turn":
                                Assert.IsNull(testState, "test state should not exist yet");

                                testState = new TestState
                                {
                                    Value = "test"
                                };

                                userState.Set(testState);

                                break;

                            case "should be null on this turn":
                                Assert.IsNull(testState, "test state should not exist because it was not saved");

                                break;
                        }
                    }
                )
                .Send("don't save on this turn")
                .Send("should be null on this turn")
                .StartTest();
        }

        [TestMethod]
        public async Task State_SavedStateIsRecalledOnNextTurn()
        {
            var adapter = new TestAdapter()
                .Use(new StateManagementMiddleware()
                        .UseDefaultStorageProvider(new MemoryStateStorageProvider())
                        .UseUserState());

            await new TestFlow(adapter,
                    async (context) =>
                    {
                        var userState = context.UserState();

                        Assert.IsNotNull(userState);

                        var testState = await userState.Get<TestState>();

                        switch (context.Activity.Text)
                        {
                            case "set value":
                                Assert.IsNull(testState, "test state should not exist yet");

                                testState = new TestState
                                {
                                    Value = "test"
                                };

                                userState.Set(testState);
                                await userState.SaveChanges();

                                break;

                            case "get value":
                                Assert.IsNotNull(testState, "test state should exist now");

                                await context.SendActivity(testState.Value);

                                break;
                        }
                    }
                )
                .Send("set value")
                .Test("get value", "test")
                .StartTest();
        }

        [TestMethod]
        public async Task State_CustomStateManagerTest()
        {
            string testStateValue = Guid.NewGuid().ToString();

            TestAdapter adapter = new TestAdapter()
                .Use(new StateManagementMiddleware()
                        .UseDefaultStorageProvider(new MemoryStateStorageProvider())
                        .UseState("custom"));

            await new TestFlow(adapter, async (context) =>
                    {
                        var customNamespaceStateManager = context.State("custom");

                        switch (context.Activity.AsMessageActivity().Text)
                        {
                            case "set value":
                                customNamespaceStateManager.Set(new TestState
                                {
                                    Value = testStateValue
                                });

                                await customNamespaceStateManager.SaveChanges();

                                await context.SendActivity("value saved");
                                break;

                            case "get value":
                                var stateEntry = await customNamespaceStateManager.Get<TestState>();

                                Assert.IsNotNull(stateEntry);

                                await context.SendActivity(stateEntry.Value);

                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", testStateValue)
                .StartTest();
        }
    }
}
