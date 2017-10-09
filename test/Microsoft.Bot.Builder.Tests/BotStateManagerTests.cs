using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class BotStateManagerTests
    {
        [TestMethod]
        public async Task DoNOTRememberContextState()
        {
            TestConnector connector = new TestConnector();
            Bot bot = new Bot(connector)
                .OnReceive( async (context, token) =>
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
            await connector.Test("set value", (a) => Assert.IsTrue(a[0].Text == "value saved", "set value failed"));
            await connector.Test("get value", (a) => Assert.IsTrue(a[0].Text == null, "get value was incorrectly defined"));
        }

        [TestMethod]
        public async Task RememberUserState()
        {
            TestConnector connector = new TestConnector();

            Bot bot = new Bot(connector)
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

            await connector.Test("set value", (a) => Assert.IsTrue(a[0].Text == "value saved"));
            await connector.Test("get value", (a) => Assert.IsTrue(a[0].Text == "test"));
        }

        [TestMethod]
        public async Task RememberConversationState()
        {
            TestConnector connector = new TestConnector();

            Bot bot = new Bot(connector)
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

            await connector.Test("set value", (a) => Assert.IsTrue(a[0].Text == "value saved"));
            await connector.Test("get value", (a) => Assert.IsTrue(a[0].Text == "test"));
        }

        [TestMethod]
        public async Task CustomStateManagerTest()
        {
            TestConnector connector = new TestConnector();
            string testGuid = Guid.NewGuid().ToString();

            Bot bot = new Bot(connector)
                .Use(new MemoryStorage())
                .Use(new CustomStateManager())
                .OnReceive(
                    async (context, token) =>
                    {
                        Assert.IsNotNull(context.State.Conversation, "state.conversation should exist");
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

            await connector.Test("set value", (a) => Assert.IsTrue(a[0].Text == "value saved"));
            await connector.Test("get value", (a) => Assert.IsTrue(a[0].Text == testGuid.ToString()));
        }

        public class CustomState : StoreItem
        {
            public string CustomString { get; set; }
        }

        public class CustomStateManager: BotStateManager
        {
            public const string KeyName = "CustomStateKey";            

            protected override async Task<StoreItems> Read(BotContext context, IList<String> keys)
            {
                keys.Add(KeyName);
                StoreItems items = await base.Read(context, keys);                

                context.State[KeyName] = items.Get<CustomState>(KeyName) ?? new CustomState() { } ;
                return items;
            }

            protected override async Task Write(BotContext context, StoreItems changes)
            {
                changes[KeyName] = context.State[KeyName];
                await base.Write(context, changes);
            }
        }
    }
}
