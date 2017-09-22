using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class RegExpRecognizerTests
    {
        [TestMethod]
        public async Task RecognizeHelpIntent()
        {
            TestConnector connector = new TestConnector();

            RegExpRecognizerMiddleare helpRecognizer = new RegExpRecognizerMiddleare()
                .AddIntent("HelpIntent", new Regex("help", RegexOptions.IgnoreCase));

            Bot bot = new Bot(connector)
                .Use(helpRecognizer);

            bot.OnReceive(async (context, token) =>
            {
                if (context.IfIntent("HelpIntent"))             
                    context.Reply("You selected HelpIntent");                

                return new ReceiveResponse(true);
            });
            
            await connector.Test("help", (a) =>
            {
                Assert.IsTrue(a[0].Type == "message");
                Assert.IsTrue(a[0].Text == "You selected HelpIntent");
            });
        }

        [TestMethod]
        public async Task RecognizeIntentViaRegex()
        {
            TestConnector connector = new TestConnector();

            RegExpRecognizerMiddleare helpRecognizer = new RegExpRecognizerMiddleare()
                .AddIntent("aaaaa", new Regex("a", RegexOptions.IgnoreCase))
                .AddIntent("bbbbb", new Regex("b", RegexOptions.IgnoreCase));

            Bot bot = new Bot(connector)
                .Use(helpRecognizer);

            bot.OnReceive(async (context, token) =>
            {
                if (context.IfIntent(new Regex("a")))
                    context.Reply("aaaa Intent");
                if (context.IfIntent(new Regex("b")))
                    context.Reply("bbbb Intent");

                return new ReceiveResponse(true);
            });

            await connector.Test("aaaaaaaaa", (a) =>
            {
                Assert.IsTrue(a[0].Type == "message");
                Assert.IsTrue(a[0].Text == "aaaa Intent");
            });

            await connector.Test("bbbbbbbbb", (a) =>
            {
                Assert.IsTrue(a[0].Type == "message");
                Assert.IsTrue(a[0].Text == "bbbb Intent");
            });
        }

        [TestMethod]
        public async Task RecognizeCancelIntent()
        {
            TestConnector connector = new TestConnector();

            RegExpRecognizerMiddleare helpRecognizer = new RegExpRecognizerMiddleare()
                .AddIntent("CancelIntent", new Regex("cancel", RegexOptions.IgnoreCase));

            Bot bot = new Bot(connector)
                .Use(helpRecognizer);

            bot.OnReceive(async (context, token) =>
            {
                if (context.IfIntent("CancelIntent"))                
                    context.Reply("You selected CancelIntent");                

                return new ReceiveResponse(true);
            });

            await connector.Test("cancel", (a) =>
            {
                Assert.IsTrue(a[0].Type == "message");
                Assert.IsTrue(a[0].Text == "You selected CancelIntent");
            });
        }

        [TestMethod]
        public async Task DoNotRecognizeCancelIntent()
        {
            TestConnector connector = new TestConnector();

            RegExpRecognizerMiddleare helpRecognizer = new RegExpRecognizerMiddleare()
                .AddIntent("CancelIntent", new Regex("cancel", RegexOptions.IgnoreCase));

            Bot bot = new Bot(connector)
                .Use(helpRecognizer);

            bot.OnReceive(async (context, token) =>
            {
                if (context.IfIntent("CancelIntent"))
                    context.Reply("You selected CancelIntent");                
                else                
                    context.Reply("Bot received request of type message");                

                return new ReceiveResponse(true);
            });

            await connector.Test("tacos", (a) =>
            {
                Assert.IsTrue(a[0].Type == "message");
                Assert.IsTrue(a[0].Text == "Bot received request of type message");
            });
        }

        [TestMethod]
        public async Task MultipleIntents()
        {
            TestConnector connector = new TestConnector();

            RegExpRecognizerMiddleare helpRecognizer = new RegExpRecognizerMiddleare()
                .AddIntent("HelpIntent", new Regex("help", RegexOptions.IgnoreCase))
                .AddIntent("CancelIntent", new Regex("cancel", RegexOptions.IgnoreCase))
                .AddIntent("TacoIntent", new Regex("taco", RegexOptions.IgnoreCase));

            Bot bot = new Bot(connector)
                .Use(helpRecognizer);

            bot.OnReceive(async (context, token) =>
            {
                if (context.IfIntent("HelpIntent"))                
                    context.Reply("You selected HelpIntent");                
                else if (context.IfIntent("CancelIntent"))
                    context.Reply("You selected CancelIntent");                
                else if (context.IfIntent("TacoIntent"))
                    context.Reply("You selected TacoIntent");                

                return new ReceiveResponse(true);
            });
            
            await connector.Test("help", (a) =>
            {
                Assert.IsTrue(a.Count == 1, "Expecting exactly 1 activity.");
                Assert.IsTrue(a[0].Type == "message");
                Assert.IsTrue(a[0].Text == "You selected HelpIntent");
            });

            await connector.Test("cancel", (a) =>
            {
                Assert.IsTrue(a.Count == 1, "Expecting exactly 1 activity.");
                Assert.IsTrue(a[0].Type == "message");
                Assert.IsTrue(a[0].Text == "You selected CancelIntent");
            });

            await connector.Test("taco", (a) =>
            {
                Assert.IsTrue(a.Count == 1, "Expecting exactly 1 activity.");
                Assert.IsTrue(a[0].Type == "message");
                Assert.IsTrue(a[0].Text == "You selected TacoIntent");
            });
        }
    }
}
