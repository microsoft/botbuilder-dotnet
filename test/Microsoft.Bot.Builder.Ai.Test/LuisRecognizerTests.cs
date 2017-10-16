using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Storage;
using System.IO;
using System.Diagnostics;

namespace Microsoft.Bot.Builder.Ai.Tests
{
    [TestClass]
    public class LuisRecognizerTests
    {
        public string luisAppId = TestUtilities.GetKey("LUISAPPID");
        public string subscriptionKey = TestUtilities.GetKey("LUISAPPKEY");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Luis")]
        public async Task Luis_TopIntentAndEntities()
        {
            if (luisAppId == null || subscriptionKey == null)
            {
                Debug.WriteLine($"Missing Luis Key- Skipping test");
                return;
            }
            LuisRecognizerMiddleware recognizer =
                new LuisRecognizerMiddleware(luisAppId, subscriptionKey);
            var context = TestUtilities.CreateEmptyContext();
            context.Request.Text = "I want a ham and cheese sandwich";

            IList<Intent> res = await recognizer.Recognize(context);
            Assert.IsTrue(res.Count == 1, "Incorrect number of intents");
            Assert.IsTrue(res[0].Name == "sandwichorder", "Incorrect Name");
            Assert.IsTrue(res[0].Entities.Count > 0, "No Entities Found");
            Assert.IsTrue(((LuisEntity)res[0].Entities[0]).Type == "meat");
            Assert.IsTrue(((LuisEntity)res[0].Entities[0]).Value == "ham");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Luis")]
        public async Task Luis_TopIntentPopulated()
        {
            if (luisAppId == null || subscriptionKey == null)
            {
                Debug.WriteLine($"Missing Luis Key- Skipping test");
                return;
            }

            TestConnector connector = new TestConnector();
            Bot bot = new Bot(connector)
                .Use(new MemoryStorage())
                .Use(new BotStateManager())
                .Use(new LuisRecognizerMiddleware(luisAppId, subscriptionKey))
                .OnReceive(async (context, token) =>
                {
                    context.Reply(context.TopIntent.Name);
                });
            await connector
                .Send("I want ham and cheese sandwich!")
                    .AssertReply("sandwichorder", "should have sandwichorder as top intent!")
                .StartTest();
        }
    }
}
