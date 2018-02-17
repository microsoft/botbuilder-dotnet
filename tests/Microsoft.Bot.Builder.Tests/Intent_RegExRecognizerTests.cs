// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Servers;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class Intent_RegExRecognizerTests
    {
        [TestMethod]
        [TestCategory("Intent Recognizers")]
        [TestCategory("RegEx Intent Recognizer")]
        public async Task Regex_RecognizeHelpIntent()
        {


            RegExpRecognizerMiddleware helpRecognizer = new RegExpRecognizerMiddleware()
                .AddIntent("HelpIntent", new Regex("help", RegexOptions.IgnoreCase));

            TestBotServer botServer = new TestBotServer()
                .Use(helpRecognizer);

            await new TestFlow(botServer, async (context) =>
                {
                    if (context.IfIntent("HelpIntent"))
                        context.Reply("You selected HelpIntent");
                })
                .Test("help", "You selected HelpIntent")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Intent Recognizers")]
        [TestCategory("RegEx Intent Recognizer")]
        public async Task Regex_ExtractEntityGroupsNamedCaptureViaList()
        {
            Regex r = new Regex(@"how (.*) (.*)", RegexOptions.IgnoreCase);
            string input = "How 11111 22222";

            Intent i = RegExpRecognizerMiddleware.Recognize(input, r, new List<string>() { "One", "Two" }, 1.0);
            Assert.IsNotNull(i, "Expected an Intent");
            Assert.IsTrue(i.Entities.Count == 2, "Should match 2 groups");
            Assert.IsTrue(i.Entities[0].ValueAs<string>() == "11111");
            Assert.IsTrue(i.Entities[0].GroupName == "One");

            Assert.IsTrue(i.Entities[1].ValueAs<string>() == "22222");
            Assert.IsTrue(i.Entities[1].GroupName == "Two");
        }

        [TestMethod]
        [TestCategory("Intent Recognizers")]
        [TestCategory("RegEx Intent Recognizer")]
        public async Task Regex_ExtractEntityGroupsNamedCaptureNoList()
        {
            Regex r = new Regex(@"how (?<One>.*) (?<Two>.*)");
            string input = "how 11111 22222";

            Intent i = RegExpRecognizerMiddleware.Recognize(input, r, 1.0);
            Assert.IsNotNull(i, "Expected an Intent");
            Assert.IsTrue(i.Entities.Count == 2, "Should match 2 groups");
            Assert.IsTrue(i.Entities[0].ValueAs<string>() == "11111");
            Assert.IsTrue(i.Entities[0].GroupName == "One");

            Assert.IsTrue(i.Entities[1].ValueAs<string>() == "22222");
            Assert.IsTrue(i.Entities[1].GroupName == "Two");
        }


        [TestMethod]
        [TestCategory("Intent Recognizers")]
        [TestCategory("RegEx Intent Recognizer")]
        public async Task Regex_RecognizeIntentViaRegex()
        {
            TestBotServer botServer = new TestBotServer()
                .Use(new RegExpRecognizerMiddleware()
                        .AddIntent("aaaaa", new Regex("a", RegexOptions.IgnoreCase))
                        .AddIntent("bbbbb", new Regex("b", RegexOptions.IgnoreCase))
                );

            await new TestFlow(botServer, async (context) =>
                {
                    if (context.IfIntent(new Regex("a")))
                        context.Reply("aaaa Intent");
                    if (context.IfIntent(new Regex("b")))
                        context.Reply("bbbb Intent");
                })
                .Test("aaaaaaaaa", "aaaa Intent")
                .Test("bbbbbbbbb", "bbbb Intent")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Intent Recognizers")]
        [TestCategory("RegEx Intent Recognizer")]
        public async Task Regex_RecognizeCancelIntent()
        {


            RegExpRecognizerMiddleware helpRecognizer = new RegExpRecognizerMiddleware()
                .AddIntent("CancelIntent", new Regex("cancel", RegexOptions.IgnoreCase));

            TestBotServer botServer = new TestBotServer()
                .Use(helpRecognizer);
            await new TestFlow(botServer, async (context) =>
                {
                    if (context.IfIntent("CancelIntent"))
                        context.Reply("You selected CancelIntent");
                })
                .Test("cancel", "You selected CancelIntent")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Intent Recognizers")]
        [TestCategory("RegEx Intent Recognizer")]
        public async Task Regex_DoNotRecognizeCancelIntent()
        {


            RegExpRecognizerMiddleware helpRecognizer = new RegExpRecognizerMiddleware()
                .AddIntent("CancelIntent", new Regex("cancel", RegexOptions.IgnoreCase));

            TestBotServer botServer = new TestBotServer()
                .Use(helpRecognizer);

            await new TestFlow(botServer, async (context) =>
                {
                    if (context.IfIntent("CancelIntent"))
                        context.Reply("You selected CancelIntent");
                    else
                        context.Reply("Bot received request of type message");
                })
                .Test("tacos", "Bot received request of type message")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Intent Recognizers")]
        [TestCategory("RegEx Intent Recognizer")]
        public async Task Regex_MultipleIntents()
        {
            RegExpRecognizerMiddleware helpRecognizer = new RegExpRecognizerMiddleware()
                .AddIntent("HelpIntent", new Regex("help", RegexOptions.IgnoreCase))
                .AddIntent("CancelIntent", new Regex("cancel", RegexOptions.IgnoreCase))
                .AddIntent("TacoIntent", new Regex("taco", RegexOptions.IgnoreCase));

            TestBotServer botServer = new TestBotServer()
                .Use(helpRecognizer);
            await new TestFlow(botServer, async (context) =>
                {
                    if (context.IfIntent("HelpIntent"))
                        context.Reply("You selected HelpIntent");
                    else if (context.IfIntent("CancelIntent"))
                        context.Reply("You selected CancelIntent");
                    else if (context.IfIntent("TacoIntent"))
                        context.Reply("You selected TacoIntent");
                })
                .Send("help").AssertReply("You selected HelpIntent")
                .Send("cancel").AssertReply("You selected CancelIntent")
                .Send("taco").AssertReply("You selected TacoIntent")
                .StartTest();
        }
    }
}
