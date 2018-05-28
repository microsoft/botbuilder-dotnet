// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Tests
{
    [TestClass]
    [TestCategory("Adapter")]
    public class TestAdapterTests
    {
        public async Task MyBotLogic(ITurnContext context)
        {
            switch (context.Activity.AsMessageActivity().Text)
            {
                case "count":
                    await context.SendActivity(context.Activity.CreateReply("one"));
                    await context.SendActivity(context.Activity.CreateReply("two"));
                    await context.SendActivity(context.Activity.CreateReply("three"));
                    break;
                case "ignore":
                    break;
                default:
                    await context.SendActivity( 
                        context.Activity.CreateReply($"echo:{context.Activity.AsMessageActivity().Text}"));
                    break;
            }
        }
        [TestMethod]
        public async Task SingleParameterConstructor()
        {
            var adapter = new TestAdapter();

            // If this compiles, the test has passed. :) 
        }

        [TestMethod]
        public async Task TestAdapter_ExceptionTypesOnTest()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter();

            try
            {
                await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity(context.Activity.CreateReply("one"));
                })
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
        public async Task TestAdapter_ExceptionInBotOnReceive()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter();

            try
            {
                await new TestFlow(adapter, async (context) => { throw new Exception(uniqueExceptionId); })
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
        public async Task TestAdapter_ExceptionTypesOnAssertReply()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter();

            try
            {
                await new TestFlow(adapter, async (context) => 
                {
                    await context.SendActivity(context.Activity.CreateReply("one"));
                })
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
        public async Task TestAdapter_SaySimple()
        {
            var adapter = new TestAdapter();
            await new TestFlow(adapter, MyBotLogic)
                .Test("foo", "echo:foo", "say with string works")
                .StartTest();
        }

        [TestMethod]
        public async Task TestAdapter_Say()
        {
            var adapter = new TestAdapter();
            await new TestFlow(adapter, MyBotLogic)
                .Test("foo", "echo:foo", "say with string works")
                .Test("foo", new Activity(ActivityTypes.Message, text: "echo:foo"), "say with activity works")
                .Test("foo", (activity) => Assert.AreEqual("echo:foo", activity.AsMessageActivity().Text), "say with validator works")
                .StartTest();
        }


        [TestMethod]
        public async Task TestAdapter_SendReply()
        {
            var adapter = new TestAdapter();
            await new TestFlow(adapter, MyBotLogic)
                .Send("foo").AssertReply("echo:foo", "send/reply with string works")
                .Send("foo").AssertReply(new Activity(ActivityTypes.Message, text: "echo:foo"), "send/reply with activity works")
                .Send("foo").AssertReply((activity) => Assert.AreEqual("echo:foo", activity.AsMessageActivity().Text), "send/reply with validator works")
                .StartTest();
        }

        [TestMethod]
        public async Task TestAdapter_ReplyOneOf()
        {
            var adapter = new TestAdapter();
            await new TestFlow(adapter, MyBotLogic)
                .Send("foo").AssertReplyOneOf(new string[] { "echo:bar", "echo:foo", "echo:blat" }, "say with string works")
                .StartTest();
        }


        [TestMethod]
        public async Task TestAdapter_MultipleReplies()
        {
            var adapter = new TestAdapter();
            await new TestFlow(adapter, MyBotLogic)
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
        public async Task TestAdapter_TestFlow(Type exceptionType)
        {
            var adapter = new TestAdapter();

            TestFlow testFlow = new TestFlow(adapter, (ctx) =>
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
