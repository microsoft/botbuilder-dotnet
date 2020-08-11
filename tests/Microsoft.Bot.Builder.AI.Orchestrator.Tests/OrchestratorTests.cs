// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.BotFramework.Orchestrator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Orchestrator.Tests
{
    [TestClass]
    public class OrchestratorTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task TestIntentRecognize()
        {
            var mockResult = new Result
            {
                Score = 0.9,
                Label = new Label
                {
                    Name = "mockLabel"
                }
            };

            var mockScore = new List<Result>() { mockResult };
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorAdaptiveRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelPath = new StringExpression("fakePath"),
                SnapshotPath = new StringExpression("fakePath")
            };

            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.AreEqual(1, result.Intents.Count);
            Assert.IsTrue(result.Intents.ContainsKey("mockLabel"));
            Assert.AreEqual(0.9, result.Intents["mockLabel"].Score);
        }

        [TestMethod]
        public async Task TestEntityRecognize()
        {
            var mockResult = new Result
            {
                Score = 0.9,
                Label = new Label
                {
                    Name = "mockLabel"
                }
            };

            var mockScore = new List<Result>() { mockResult };
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorAdaptiveRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelPath = new StringExpression("fakePath"),
                SnapshotPath = new StringExpression("fakePath")
            };
            recognizer.EntityRecognizers.Add(new NumberEntityRecognizer());

            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("12");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.IsNotNull(result.Entities);
            Assert.AreEqual(result.Entities["number"][0], new JValue("12"));
            var resolution = result.Entities["$instance"]["number"][0]["resolution"];
            Assert.AreEqual(resolution["subtype"], new JValue("integer"));
            Assert.AreEqual(resolution["value"], new JValue("12"));
        }

        [TestMethod]
        public async Task TestAmbiguousResults()
        {
            var mockResult1 = new Result
            {
                Score = 0.61,
                Label = new Label
                {
                    Name = "mockLabel1"
                }
            };

            var mockResult2 = new Result
            {
                Score = 0.62,
                Label = new Label
                {
                    Name = "mockLabel2"
                }
            };

            var mockScore = new List<Result>() { mockResult1, mockResult2 };
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorAdaptiveRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelPath = new StringExpression("fakePath"),
                SnapshotPath = new StringExpression("fakePath"),
                DetectAmbiguousIntents = new BoolExpression(true),
                DisambiguationScoreThreshold = new NumberExpression(0.5)
            };

            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("12");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.IsTrue(result.Intents.ContainsKey("ChooseIntent"));
        }
    }
}
