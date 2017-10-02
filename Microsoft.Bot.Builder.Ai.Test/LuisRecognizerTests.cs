using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Storage;

namespace Microsoft.Bot.Builder.Ai.Tests
{
    [TestClass]
    public class LuisRecognizerTests
    {
        // Taken from the JS SDK File "LuisRecognizerTests.js"
        public const string luisAppId = "165ecd1b-5643-4914-aea8-cddcf2b5d9e3";
        public const string subscriptionKey = "a5f892758b174b32b6d87b7b58f09477";


        [TestMethod]
        public async Task TopIntentAndEntities()
        {
            LuisRecognizerMiddleware recognizer = new LuisRecognizerMiddleware(luisAppId, subscriptionKey);
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
        public async Task TopIntentPopulated()
        {
            TestConnector connector = new TestConnector();
            Bot bot = new Bot(connector)
                .Use(new MemoryStorage())
                .Use(new BotStateManager())
                .Use(new LuisRecognizerMiddleware(luisAppId, subscriptionKey))
                .OnReceive(async (context, token) => {
                    context.Reply(context.TopIntent.Name);
                    return new ReceiveResponse(true);
                });
            await connector.Test("I want ham and cheese sandwich!", 
                (a) => Assert.IsTrue(a[0].Text == "sandwichorder", "should have sandwichorder as top intent!"));            
        }
    }
}
