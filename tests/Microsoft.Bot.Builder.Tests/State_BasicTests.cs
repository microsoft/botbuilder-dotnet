// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("State Management")]
    public class State_BasicTests
    {
        [TestMethod]        
        public async Task State_DoNOTRememberContextState()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter);
            bot.OnReceive(async (context) =>
                   {
                       Assert.IsNotNull(context.State, "context.state should exist");
                       switch (context.Request.AsMessageActivity().Text)
                       {
                           case "set value":
                               context.State["value"] = "test";
                               context.Reply("value saved");
                               break;
                           case "get value":
                               string state = context.State["value"];
                               context.Reply(state);
                               break;
                       }               
                   }
                );
            await adapter.Test("set value", "value saved", "set value failed")
                .Test("get value", (a) => Assert.IsTrue(a.AsMessageActivity().Text == null, "get value was incorrectly defined"))
                .StartTest();
        }

        [TestMethod]
        public async Task State_RememberUserState()
        {
            TestAdapter adapter = new TestAdapter();

            Bot bot = new Bot(adapter)
                .Use(new UserStateManagerMiddleware(new MemoryStorage()));
            bot.OnReceive(
                    async (context) =>
                    {
                        Assert.IsNotNull(context.State.UserProperties, "state.user should exist");
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                context.State.UserProperties["value"] = "test";
                                context.Reply("value saved");
                                break;
                            case "get value":
                                context.Reply(context.State.UserProperties["value"]);
                                break;
                        }
                    }
                );

            await adapter.Test("set value", "value saved")
                .Test("get value", "test")
                .StartTest();
        }

        [TestMethod]
        public async Task State_RememberConversationState()
        {
            TestAdapter adapter = new TestAdapter();

            Bot bot = new Bot(adapter)
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));
            bot.OnReceive(
                    async (context) =>
                    {
                        Assert.IsNotNull(context.State.ConversationProperties, "state.conversation should exist");
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                context.State.ConversationProperties["value"] = "test";
                                context.Reply("value saved");
                                break;
                            case "get value":
                                context.Reply(context.State.ConversationProperties["value"]);
                                break;
                        }
                    }
                );

            await adapter.Test("set value", "value saved")
                .Test("get value", "test")
                .StartTest();
        }

        [TestMethod]
        public async Task State_CustomStateManagerTest()
        {
            TestAdapter adapter = new TestAdapter();
            string testGuid = Guid.NewGuid().ToString();

            Bot bot = new Bot(adapter)
                .Use(new CustomConversationStateManager(new MemoryStorage()));
            bot.OnReceive(async (context) =>
                    {
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                context.State[CustomConversationStateManager.KeyName].CustomString = testGuid;
                                context.Reply("value saved");
                                break;
                            case "get value":
                                context.Reply(context.State[CustomConversationStateManager.KeyName].CustomString);
                                break;
                        }
                    }
                );

            await adapter.Test("set value", "value saved")
                .Test("get value", testGuid.ToString())
                .StartTest();
        }

        public class TypedObject
        {
            public string Name { get; set; }
        }

        [TestMethod]
        public async Task State_RoundTripTypedObject()
        {
            TestAdapter adapter = new TestAdapter();

            Bot bot = new Bot(adapter)
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));
            bot.OnReceive(
                    async (context) =>
                    {
                        Assert.IsNotNull(context.State.ConversationProperties, "state.conversation should exist");
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                context.State.ConversationProperties["value"] = new TypedObject() { Name = "test" };
                                context.Reply("value saved");
                                break;
                            case "get value":
                                context.Reply(context.State.ConversationProperties["value"].GetType().Name);
                                break;
                        }                        
                    }
                );

            await adapter.Test("set value", "value saved")
                .Test("get value", "TypedObject")
                .StartTest();
        }

        public class CustomState : StoreItem
        {
            public string CustomString { get; set; }
        }

        public class CustomConversationStateManager : ConversationStateManagerMiddleware
        {
            public const string KeyName = "CustomConversationStateKey";
            public CustomConversationStateManager(IStorage storage) : base(storage)
            {
            }

            protected override async Task<StoreItems> Read(IBotContext context, IList<String> keys = null)
            {
                if (keys == null)
                    keys = new List<String>();

                keys.Add(KeyName);
                StoreItems items = await base.Read(context, keys);

                context.State[KeyName] = items.Get<CustomState>(KeyName) ?? new CustomState() { };
                return items;
            }

            protected override async Task Write(IBotContext context, StoreItems changes = null)
            {
                if (changes == null)
                    changes = new StoreItems();

                changes[KeyName] = context.State[KeyName];
                await base.Write(context, changes);
            }
        }
    }
}
