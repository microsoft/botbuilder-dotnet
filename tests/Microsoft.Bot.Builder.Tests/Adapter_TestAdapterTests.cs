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
    public class Adapter_TestAdapterTests
    {
        private TestAdapter CreateAdapter()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter);
            bot.OnReceive(
                    async (context) =>
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
                );
            return adapter;
        }


        [TestMethod]
        public async Task TestAdapter_ExceptionTypesOnTest()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter);
            bot.OnReceive(async (context) => { context.Reply("one"); });

            try
            {
                await adapter
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
        public async Task TestAdapter_ExceptionTypesOnAssertReply()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter);
            bot.OnReceive(async (context) => { context.Reply("one"); });

            try
            {
                await adapter
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
        public async Task TestAdapter_Say()
        {
            var adapter = this.CreateAdapter();
            await adapter
                .Test("foo", "echo:foo", "say with string works")
                .Test("foo", new Activity(ActivityTypes.Message, text: "echo:foo"), "say with activity works")
                .Test("foo", (activity) => Assert.AreEqual("echo:foo", activity.AsMessageActivity().Text), "say with validator works")
                .StartTest();
        }


        [TestMethod]
        public async Task TestAdapter_SendReply()
        {
            var adapter = this.CreateAdapter();
            await adapter
                .Send("foo").AssertReply("echo:foo", "send/reply with string works")
                .Send("foo").AssertReply(new Activity(ActivityTypes.Message, text: "echo:foo"), "send/reply with activity works")
                .Send("foo").AssertReply((activity) => Assert.AreEqual("echo:foo", activity.AsMessageActivity().Text), "send/reply with validator works")
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

        [TestMethod]
        [DataRow(typeof(SecurityException))]
        [DataRow(typeof(ArgumentException))]
        [DataRow(typeof(ArgumentNullException))]
        public async Task TestAdapter_TestFlow(Type exceptionType)
        {
            Exception innerException = (Exception)Activator.CreateInstance(exceptionType);
            var promise = new TaskCompletionSource<bool>();
            promise.SetException(innerException);
            var adapter = this.CreateAdapter();

            TestFlow testFlow = new TestFlow(promise.Task, adapter);
            testFlow.Send(new Activity());
            Task task = testFlow.StartTest();
            await task.ContinueWith(action =>
            {
                Assert.IsInstanceOfType(action.Exception.InnerException, exceptionType);
            });
        }

    }
}
