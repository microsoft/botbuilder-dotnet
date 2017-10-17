using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

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
            Bot bot = new Bot(adapter)
                .OnReceive(async (context, token) =>
                   {
                       Assert.IsNotNull(context.State, "context.state should exist");
                       switch (context.Request.Text)
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
                .Test("get value", (a) => Assert.IsTrue(a.Text == null, "get value was incorrectly defined"))
                .StartTest();
        }

        [TestMethod]
        public async Task State_RememberUserState()
        {
            TestAdapter adapter = new TestAdapter();

            Bot bot = new Bot(adapter)
                .Use(new MemoryStorage())
                .Use(new BotStateManager())
                .OnReceive(
                    async (context, token) =>
                    {
                        Assert.IsNotNull(context.State.User, "state.user should exist");
                        switch (context.Request.Text)
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
                .Use(new MemoryStorage())
                .Use(new BotStateManager())
                .OnReceive(
                    async (context, token) =>
                    {
                        Assert.IsNotNull(context.State.Conversation, "state.conversation should exist");
                        switch (context.Request.Text)
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
                .Use(new MemoryStorage())
                .Use(new CustomStateManager())
                .OnReceive(
                    async (context, token) =>
                    {
                        switch (context.Request.Text)
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
                );

            await adapter.Test("set value", "value saved")
                .Test("get value", testGuid.ToString())
                .StartTest();
        }

        public class CustomState : StoreItem
        {
            public string CustomString { get; set; }
        }

        public class CustomStateManager : BotStateManager
        {
            public const string KeyName = "CustomStateKey";

            protected override async Task<StoreItems> Read(BotContext context, IList<String> keys = null)
            {
                if (keys == null)
                    keys = new List<String>();

                keys.Add(KeyName);
                StoreItems items = await base.Read(context, keys);

                context.State[KeyName] = items.Get<CustomState>(KeyName) ?? new CustomState() { };
                return items;
            }

            protected override async Task Write(BotContext context, StoreItems changes = null)
            {
                if (changes == null)
                    changes = new StoreItems();

                changes[KeyName] = context.State[KeyName];
                await base.Write(context, changes);
            }
        }
    }
}
