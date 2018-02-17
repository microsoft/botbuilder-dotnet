// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Servers;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            context.Request.AsMessageActivity().Text = "I want a ham and cheese sandwich";

            IList<Middleware.Intent> res = await recognizer.Recognize(context);
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

            
            TestBotServer botServer = new TestBotServer()
                .Use(new LuisRecognizerMiddleware(luisAppId, subscriptionKey));
            await new TestFlow(botServer, (context) =>
                {
                    context.Reply(context.TopIntent.Name);
                    return Task.CompletedTask;
                })
                .Send("I want ham and cheese sandwich!")
                    .AssertReply("sandwichorder", "should have sandwichorder as top intent!")
                .StartTest();
        }
    }
}
