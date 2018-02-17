// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using System.Security;
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
        public async Task TestBot_ExceptionTypesOnTest()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestBot bot = new TestBot();

            try
            {
                await new TestFlow(bot, async (context) => { context.Reply("one"); })
                    .Test("foo", (activity) => throw new Exception(uniqueExceptionId))
                    .StartTest();

                Assert.Fail("An Exception should have been thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message == uniqueExceptionId, "Incorrect Exception Text");
            }
        }

        [TestMethod]
        public async Task TestBot_ExceptionInBotOnReceive()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestBot bot = new TestBot();

            try
            {
                await new TestFlow(bot, async (context) => { throw new Exception(uniqueExceptionId); })
                    .Test("test", activity => Assert.IsNull(null), "uh oh!")
                    .StartTest();

                Assert.Fail("An Exception should have been thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.InnerException.Message == uniqueExceptionId, "Incorrect Exception Text");
            }
        }

        [TestMethod]
        public async Task TestBot_ExceptionTypesOnAssertReply()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestBot bot = new TestBot();

            try
            {
                await new TestFlow(bot, async (context) => { context.Reply("one"); })
                    .Send("foo")
                    .AssertReply(
                        (activity) => throw new Exception(uniqueExceptionId), "should throw")
                    .StartTest();

                Assert.Fail("An Exception should have been thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message == uniqueExceptionId, "Incorrect Exception Text");
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

        [DataTestMethod]
        [DataRow(typeof(SecurityException))]
        [DataRow(typeof(ArgumentException))]
        [DataRow(typeof(ArgumentNullException))]
        public async Task TestBot_TestFlow(Type exceptionType)
        {
            var bot = new TestBot();

            TestFlow testFlow = new TestFlow(bot, (ctx) =>
                {
                    Exception innerException = (Exception)Activator.CreateInstance(exceptionType);
                    var taskSource = new TaskCompletionSource<bool>();
                    taskSource.SetException(innerException);
                    return taskSource.Task;
                })
                .Send(new Activity());
                Task task = testFlow.StartTest()
                    .ContinueWith(action =>
                    {
                        Assert.IsInstanceOfType(action.Exception.InnerException, exceptionType);
                    });
        }
    }
}
