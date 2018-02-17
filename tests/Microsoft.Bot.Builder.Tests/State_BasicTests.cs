// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Servers;
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

            TestBotServer botServer = new TestBotServer();

            await new TestFlow(botServer, async (context) =>
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
                )
                .Test("set value", "value saved", "set value failed")
                .Test("get value", (a) => Assert.IsTrue(a.AsMessageActivity().Text == null, "get value was incorrectly defined"))
                .StartTest();
        }

        [TestMethod]
        public async Task State_RememberUserState()
        {
            

            var botServer = new TestBotServer()
                .Use(new BotStateManager(new MemoryStorage()));
            await new TestFlow(botServer, 
                    async (context) =>
                    {
                        Assert.IsNotNull(context.State.User, "state.user should exist");
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                context.State.User["value"] = "test";
                                context.Reply("value saved");
                                break;
                            case "get value":
                                context.Reply(context.State.User["value"]);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTest();
        }

        [TestMethod]
        public async Task State_RememberConversationState()
        {
            

            TestBotServer botServer= new TestBotServer()
                .Use(new BotStateManager(new MemoryStorage()));
            await new TestFlow(botServer, 
                    async (context) =>
                    {
                        Assert.IsNotNull(context.State.Conversation, "state.conversation should exist");
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                context.State.Conversation["value"] = "test";
                                context.Reply("value saved");
                                break;
                            case "get value":
                                context.Reply(context.State.Conversation["value"]);
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

            TestBotServer botServer = new TestBotServer()
                .Use(new CustomStateManager(new MemoryStorage()));
            await new TestFlow(botServer, async (context) =>
                    {
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                context.State[CustomStateManager.KeyName].CustomString = testGuid;
                                context.Reply("value saved");
                                break;
                            case "get value":
                                context.Reply(context.State[CustomStateManager.KeyName].CustomString);
                                break;
                        }
                    }
                )
                .Test("set value", "value saved")
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
            TestBotServer botServer= new TestBotServer()
                .Use(new BotStateManager(new MemoryStorage()));

            await new TestFlow(botServer, 
                    async (context) =>
                    {
                        Assert.IsNotNull(context.State.Conversation, "state.conversation should exist");
                        switch (context.Request.AsMessageActivity().Text)
                        {
                            case "set value":
                                context.State.Conversation["value"] = new TypedObject() { Name = "test" };
                                context.Reply("value saved");
                                break;
                            case "get value":
                                context.Reply(context.State.Conversation["value"].GetType().Name);
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

        public class CustomStateManager : BotStateManager
        {
            public const string KeyName = "CustomStateKey";
            public CustomStateManager(IStorage storage) : base(storage)
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
