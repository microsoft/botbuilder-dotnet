using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Connector")]
    public class Connector_TestConnectorTests
    {
        [TestMethod]
        public TestConnector CreateConnector()
        {
            TestConnector connector = new TestConnector();
            Bot bot = new Bot(connector)
                .OnReceive(
                    async (context, token) =>
                    {
                        Assert.IsNotNull(context.State.User, "state.user should exist");
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
            return connector;
        }

        [TestMethod]
        public async Task TestConnector_Say()
        {
            var connector = this.CreateConnector();
            await connector
                .Say("foo", "echo:foo", "say with string works")
                .Say("foo", new Activity(ActivityTypes.Message, text: "echo:foo"), "say with activity works")
                .Say("foo", (activity) => Assert.AreEqual("echo:foo", activity.Text), "say with validator works")
                .StartTest();
        }


        [TestMethod]
        public async Task TestConnector_SendReply()
        {
            var connector = this.CreateConnector();
            await connector
                .Send("foo").Reply("echo:foo", "send/reply with string works")
                .Send("foo").Reply(new Activity(ActivityTypes.Message, text: "echo:foo"), "send/reply with activity works")
                .Send("foo").Reply((activity) => Assert.AreEqual("echo:foo", activity.Text), "send/reply with validator works")
                .StartTest();
        }

        [TestMethod]
        public async Task TestConnector_ReplyOneOf()
        {
            var connector = this.CreateConnector();
            await connector
                .Send("foo").ReplyOneOf(new string[] { "echo:bar", "echo:foo", "echo:blat" }, "say with string works")
                .StartTest();
        }


        [TestMethod]
        public async Task TestConnector_MultipleReplies()
        {
            var connector = this.CreateConnector();
            await connector
                .Send("foo").Reply("echo:foo")
                .Send("bar").Reply("echo:bar")
                .Send("ignore")
                .Send("count")
                    .Reply("one")
                    .Reply("two")
                    .Reply("three")
                .StartTest();
        }
    }
}
