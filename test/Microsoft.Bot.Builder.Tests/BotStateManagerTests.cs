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
        public async Task RememberUserState()
        {
            string messageText = Guid.NewGuid().ToString();

            ValidateOnPostConnector connector = new ValidateOnPostConnector();

            Bot bot = new Bot(connector)
                .Use(new MemoryStorage())
                .Use(new BotStateManager())
                .OnReceive(
                    async (context, token) =>
                    {
                        Assert.IsNotNull(context.State.User, "state.user should exist");
                        switch(context.Request.Text)
                        {
                            case "set value":
                                context.State.User["value"] = "test";
                                context.Say("value saved");
                                break;
                            case "get value":
                                context.Say(context.State.User["value"]);
                                break;
                        }
                        return new ReceiveResponse(true);
                    }
                );

            var runner = new TestRunner();

            await runner.Test(connector, "set value", (a) => Assert.IsTrue(a[0].Text == "value saved"));
            await runner.Test(connector, "get value", (a) => Assert.IsTrue(a[0].Text == "test"));

        }
    }
}
