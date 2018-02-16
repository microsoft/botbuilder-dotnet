// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Adapter")]
    public class Adapter_TestBotTests
    {
        public async Task MyBotLogic(IBotContext context)
        {
            switch (context.Request.AsMessageActivity().Text)
            {
                case "count":
                    context.Reply("one");
                    context.Reply("two");
                    context.Reply("three");
                    break;
                case "ignore":
                    break;
                default:
                    context.Reply($"echo:{context.Request.AsMessageActivity().Text}");
                    break;
            }
        }
    
        [TestMethod]
        public async Task TestBot_Say()
        {
            var bot = new TestBot();
            await new TestFlow(bot, MyBotLogic)
                .Test("foo", "echo:foo", "say with string works")
                .Test("foo", new Activity(ActivityTypes.Message, text: "echo:foo"), "say with activity works")
                .Test("foo", (activity) => Assert.AreEqual("echo:foo", activity.AsMessageActivity().Text), "say with validator works")
                .StartTest();
        }


        [TestMethod]
        public async Task TestBot_SendReply()
        {
            var bot = new TestBot();
            await new TestFlow(bot, MyBotLogic)
                .Send("foo").AssertReply("echo:foo", "send/reply with string works")
                .Send("foo").AssertReply(new Activity(ActivityTypes.Message, text: "echo:foo"), "send/reply with activity works")
                .Send("foo").AssertReply((activity) => Assert.AreEqual("echo:foo", activity.AsMessageActivity().Text), "send/reply with validator works")
                .StartTest();
        }

        [TestMethod]
        public async Task TestBot_ReplyOneOf()
        {
            var bot = new TestBot();
            await new TestFlow(bot, MyBotLogic)
                .Send("foo").AssertReplyOneOf(new string[] { "echo:bar", "echo:foo", "echo:blat" }, "say with string works")
                .StartTest();
        }


        [TestMethod]
        public async Task TestBot_MultipleReplies()
        {
            var bot = new TestBot();
            await new TestFlow(bot, MyBotLogic)
                .Send("foo").AssertReply("echo:foo")
                .Send("bar").AssertReply("echo:bar")
                .Send("ignore")
                .Send("count")
                    .AssertReply("one")
                    .AssertReply("two")
                    .AssertReply("three")
                .StartTest();
        }
    }
}
