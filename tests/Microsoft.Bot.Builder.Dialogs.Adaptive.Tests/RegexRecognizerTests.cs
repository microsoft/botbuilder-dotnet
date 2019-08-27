// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class RegexRecognizerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task RegexRecognizer_Intent()
        {
            var recognizer = new RegexRecognizer()
            {
                Intents = new Dictionary<string, string>()
                 {
                     { "codeIntent", "(?<code>[a-z][0-9])" },
                     { "colorIntent", "(?<color>(red|orange|yellow|green|blue|indigo|violet))" }
                 }
            };
            var tc = CreateContext("intent a1 b2");

            var result = await recognizer.RecognizeAsync(tc, CancellationToken.None);

            // intent assertions
            Assert.AreEqual(1, result.Intents.Count, "Should recognize one intent");
            Assert.AreEqual("codeIntent", result.Intents.Select(i => i.Key).First(), "Should recognize codeIntent");

            // entity assertions from capture group
            dynamic entities = result.Entities;
            Assert.IsNotNull(entities.code, "should find code");
            Assert.IsNull(entities.color, "should not find color");
            Assert.AreEqual(2, entities.code.Count, "should find 2 codes");
            Assert.AreEqual("a1", (string)entities.code[0], "should find a1");
            Assert.AreEqual("b2", (string)entities.code[1], "should find b2");

            tc = CreateContext("red and orange");

            // intent assertions
            result = await recognizer.RecognizeAsync(tc, CancellationToken.None);
            Assert.AreEqual(1, result.Intents.Count, "Should recognize one intent");
            Assert.AreEqual("colorIntent", result.Intents.Select(i => i.Key).First(), "Should recognize colorIntent");

            // entity assertions from capture group
            entities = result.Entities;
            Assert.IsNotNull(entities.color, "should find color");
            Assert.IsNull(entities.code, "should not find code");
            Assert.AreEqual(2, entities.color.Count, "should find 2 colors");
            Assert.AreEqual("red", (string)entities.color[0], "should find red");
            Assert.AreEqual("orange", (string)entities.color[1], "should find orange");
        }

        private static TurnContext CreateContext(string text)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = text;
            return new TurnContext(new TestAdapter(), (Activity)activity);
        }
    }
}
