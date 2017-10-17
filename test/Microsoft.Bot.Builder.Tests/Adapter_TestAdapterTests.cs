using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Adapter")]
    public class Adapter_TestAdapterTests
    {
        private TestAdapter CreateAdapter()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .OnReceive(
                    async (context, token) =>
                    {
                        switch (context.Request.Text)
                        {
                            case "count":
                                context.Reply("one");
                                context.Reply("two");
                                context.Reply("three");
                                break;
                            case "ignore":
                                break;
                            default:
                                context.Reply($"echo:{context.Request.Text}");
                                break;
                        }
                    }
                );
            return adapter;
        }

        [TestMethod]
        public async Task TestAdapter_Say()
        {
            var adapter = this.CreateAdapter();
            await adapter
                .Test("foo", "echo:foo", "say with string works")
                .Test("foo", new Activity(ActivityTypes.Message, text: "echo:foo"), "say with activity works")
                .Test("foo", (activity) => Assert.AreEqual("echo:foo", activity.Text), "say with validator works")
                .StartTest();
        }


        [TestMethod]
        public async Task TestAdapter_SendReply()
        {
            var adapter = this.CreateAdapter();
            await adapter
                .Send("foo").AssertReply("echo:foo", "send/reply with string works")
                .Send("foo").AssertReply(new Activity(ActivityTypes.Message, text: "echo:foo"), "send/reply with activity works")
                .Send("foo").AssertReply((activity) => Assert.AreEqual("echo:foo", activity.Text), "send/reply with validator works")
                .StartTest();
        }

        [TestMethod]
        public async Task TestAdapter_ReplyOneOf()
        {
            var adapter = this.CreateAdapter();
            await adapter
                .Send("foo").AssertReplyOneOf(new string[] { "echo:bar", "echo:foo", "echo:blat" }, "say with string works")
                .StartTest();
        }


        [TestMethod]
        public async Task TestAdapter_MultipleReplies()
        {
            var adapter = this.CreateAdapter();
            await adapter
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
