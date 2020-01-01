// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    public class TurnContextTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullAdapter()
        {
            var c = new TurnContext(null, new Activity());
            Assert.Fail("Should Fail due to null Adapter");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullActivity()
        {
            var a = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            var c = new TurnContext(a, null);
            Assert.Fail("Should Fail due to null Activity");
        }

        [TestMethod]
        public void Constructor()
        {
            var c = new TurnContext(new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName)), new Activity());
            Assert.IsNotNull(c);
        }

        [TestMethod]
        public void RespondedIsFalse()
        {
            var c = new TurnContext(new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName)), new Activity());
            Assert.IsFalse(c.Responded);
        }

        [TestMethod]
        public async Task CacheValueUsingSetAndGet()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            await new TestFlow(adapter, MyBotLogic)
                    .Send("TestResponded")
                    .StartTestAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetThrowsOnNullKey()
        {
            var c = new TurnContext(new SimpleAdapter(), new Activity());
            c.TurnState.Get<object>(null);
        }

        [TestMethod]
        public void GetReturnsNullOnEmptyKey()
        {
            var c = new TurnContext(new SimpleAdapter(), new Activity());
            var service = c.TurnState.Get<object>(string.Empty); // empty key
            Assert.IsNull(service, "Should not have found a service under an empty key");
        }

        [TestMethod]
        public void GetReturnsNullWithUnknownKey()
        {
            var c = new TurnContext(new SimpleAdapter(), new Activity());
            var o = c.TurnState.Get<object>("test");
            Assert.IsNull(o);
        }

        [TestMethod]
        public void CacheValueUsingGetAndSet()
        {
            var c = new TurnContext(new SimpleAdapter(), new Activity());

            c.TurnState.Add("bar", "foo");
            var result = c.TurnState.Get<string>("bar");

            Assert.AreEqual("foo", result);
        }

        [TestMethod]
        public void CacheValueUsingGetAndSetGenericWithTypeAsKeyName()
        {
            var c = new TurnContext(new SimpleAdapter(), new Activity());

            c.TurnState.Add<string>("foo");
            string result = c.TurnState.Get<string>();

            Assert.AreEqual("foo", result);
        }

        [TestMethod]
        public void RequestIsSet()
        {
            var c = new TurnContext(new SimpleAdapter(), TestMessage.Message());
            Assert.IsTrue(c.Activity.Id == "1234");
        }

        [TestMethod]
        public async Task SendAndSetResponded()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());
            Assert.IsFalse(c.Responded);
            var response = await c.SendActivityAsync(TestMessage.Message("testtest"));

            Assert.IsTrue(c.Responded);
            Assert.IsTrue(response.Id == "testtest");
        }

        [TestMethod]
        public async Task SendBatchOfActivities()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());
            Assert.IsFalse(c.Responded);

            var message1 = TestMessage.Message("message1");
            var message2 = TestMessage.Message("message2");

            var response = await c.SendActivitiesAsync(new IActivity[] { message1, message2 });

            Assert.IsTrue(c.Responded);
            Assert.IsTrue(response.Length == 2);
            Assert.IsTrue(response[0].Id == "message1");
            Assert.IsTrue(response[1].Id == "message2");
        }

        [TestMethod]
        public async Task SendAndSetRespondedUsingIMessageActivity()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());
            Assert.IsFalse(c.Responded);

            var msg = TestMessage.Message().AsMessageActivity();
            await c.SendActivityAsync(msg);
            Assert.IsTrue(c.Responded);
        }

        [TestMethod]
        public async Task TraceActivitiesDoNoSetResponded()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());
            Assert.IsFalse(c.Responded);

            // Send a Trace Activity, and make sure responded is NOT set. 
            var trace = Activity.CreateTraceActivity("trace");
            await c.SendActivityAsync(trace);
            Assert.IsFalse(c.Responded);

            // Just to sanity check everything, send a Message and verify the
            // responded flag IS set.
            var msg = TestMessage.Message().AsMessageActivity();
            await c.SendActivityAsync(msg);
            Assert.IsTrue(c.Responded);
        }

        [TestMethod]
        public async Task SendOneActivityToAdapter()
        {
            bool foundActivity = false;

            void ValidateResponses(Activity[] activities)
            {
                Assert.IsTrue(activities.Count() == 1, "Incorrect Count");
                Assert.IsTrue(activities[0].Id == "1234");
                foundActivity = true;
            }

            var a = new SimpleAdapter(ValidateResponses);
            var c = new TurnContext(a, new Activity());
            await c.SendActivityAsync(TestMessage.Message());
            Assert.IsTrue(foundActivity);
        }

        [TestMethod]
        public async Task CallOnSendBeforeDelivery()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());

            int count = 0;
            c.OnSendActivities(async (context, activities, next) =>
            {
                Assert.IsNotNull(activities, "Null Array passed in");
                count = activities.Count();
                return await next();
            });

            await c.SendActivityAsync(TestMessage.Message());

            Assert.IsTrue(count == 1);
        }

        [TestMethod]
        public async Task AllowInterceptionOfDeliveryOnSend()
        {
            bool responsesSent = false;
            void ValidateResponses(Activity[] activities)
            {
                responsesSent = true;
                Assert.Fail("Should not be called. Interceptor did not work");
            }

            var a = new SimpleAdapter(ValidateResponses);
            var c = new TurnContext(a, new Activity());

            int count = 0;
            c.OnSendActivities((context, activities, next) =>
            {
                Assert.IsNotNull(activities, "Null Array passed in");
                count = activities.Count();

                // Do not call next.
                return Task.FromResult<ResourceResponse[]>(null);
            });

            await c.SendActivityAsync(TestMessage.Message());

            Assert.IsTrue(count == 1);
            Assert.IsFalse(responsesSent, "Responses made it to the adapter.");
        }

        [TestMethod]
        public async Task InterceptAndMutateOnSend()
        {
            bool foundIt = false;
            void ValidateResponses(Activity[] activities)
            {
                Assert.IsNotNull(activities);
                Assert.IsTrue(activities.Length == 1);
                Assert.IsTrue(activities[0].Id == "changed");
                foundIt = true;
            }

            var a = new SimpleAdapter(ValidateResponses);
            var c = new TurnContext(a, new Activity());

            c.OnSendActivities(async (context, activities, next) =>
            {
                Assert.IsNotNull(activities, "Null Array passed in");
                Assert.IsTrue(activities.Count() == 1);
                Assert.IsTrue(activities[0].Id == "1234", "Unknown Id Passed In");
                activities[0].Id = "changed";
                return await next();
            });

            await c.SendActivityAsync(TestMessage.Message());

            // Intercepted the message, changed it, and sent it on to the Adapter
            Assert.IsTrue(foundIt);
        }

        [TestMethod]
        public async Task UpdateOneActivityToAdapter()
        {
            bool foundActivity = false;

            void ValidateUpdate(Activity activity)
            {
                Assert.IsNotNull(activity);
                Assert.IsTrue(activity.Id == "test");
                foundActivity = true;
            }

            var a = new SimpleAdapter(ValidateUpdate);
            var c = new TurnContext(a, new Activity());

            var message = TestMessage.Message("test");
            var updateResult = await c.UpdateActivityAsync(message);

            Assert.IsTrue(foundActivity);
            Assert.IsTrue(updateResult.Id == "test");
        }

        [TestMethod]
        public async Task UpdateActivityWithMessageFactory()
        {
            const string ACTIVITY_ID = "activity ID";
            const string CONVERSATION_ID = "conversation ID";

            var foundActivity = false;

            void ValidateUpdate(Activity activity)
            {
                Assert.IsNotNull(activity);
                Assert.IsTrue(activity.Id == ACTIVITY_ID);
                Assert.IsTrue(activity.Conversation.Id == CONVERSATION_ID);
                foundActivity = true;
            }

            var a = new SimpleAdapter(ValidateUpdate);
            var c = new TurnContext(a, new Activity(conversation: new ConversationAccount(id: CONVERSATION_ID)));

            var message = MessageFactory.Text("test text");

            message.Id = ACTIVITY_ID;

            var updateResult = await c.UpdateActivityAsync(message);

            Assert.IsTrue(foundActivity);
            Assert.IsTrue(updateResult.Id == ACTIVITY_ID);

            c.Dispose();
        }

        [TestMethod]
        public async Task CallOnUpdateBeforeDelivery()
        {
            bool foundActivity = false;

            void ValidateUpdate(Activity activity)
            {
                Assert.IsNotNull(activity);
                Assert.IsTrue(activity.Id == "1234");
                foundActivity = true;
            }

            SimpleAdapter a = new SimpleAdapter(ValidateUpdate);
            TurnContext c = new TurnContext(a, new Activity());

            bool wasCalled = false;
            c.OnUpdateActivity(async (context, activity, next) =>
            {
                Assert.IsNotNull(activity, "Null activity passed in");
                wasCalled = true;
                return await next();
            });
            await c.UpdateActivityAsync(TestMessage.Message());
            Assert.IsTrue(wasCalled);
            Assert.IsTrue(foundActivity);
        }

        [TestMethod]
        public async Task InterceptOnUpdate()
        {
            bool adapterCalled = false;
            void ValidateUpdate(Activity activity)
            {
                adapterCalled = true;
                Assert.Fail("Should not be called.");
            }

            var a = new SimpleAdapter(ValidateUpdate);
            var c = new TurnContext(a, new Activity());

            bool wasCalled = false;
            c.OnUpdateActivity((context, activity, next) =>
            {
                Assert.IsNotNull(activity, "Null activity passed in");
                wasCalled = true;

                // Do Not Call Next
                return Task.FromResult<ResourceResponse>(null);
            });

            await c.UpdateActivityAsync(TestMessage.Message());
            Assert.IsTrue(wasCalled); // Interceptor was called
            Assert.IsFalse(adapterCalled); // Adapter was not
        }

        [TestMethod]
        public async Task InterceptAndMutateOnUpdate()
        {
            bool adapterCalled = false;
            void ValidateUpdate(Activity activity)
            {
                Assert.IsTrue(activity.Id == "mutated");
                adapterCalled = true;
            }

            var a = new SimpleAdapter(ValidateUpdate);
            var c = new TurnContext(a, new Activity());

            c.OnUpdateActivity(async (context, activity, next) =>
            {
                Assert.IsNotNull(activity, "Null activity passed in");
                Assert.IsTrue(activity.Id == "1234");
                activity.Id = "mutated";
                return await next();
            });

            await c.UpdateActivityAsync(TestMessage.Message());
            Assert.IsTrue(adapterCalled); // Adapter was not
        }

        [TestMethod]
        public async Task DeleteOneActivityToAdapter()
        {
            bool deleteCalled = false;

            void ValidateDelete(ConversationReference r)
            {
                Assert.IsNotNull(r);
                Assert.IsTrue(r.ActivityId == "12345");
                deleteCalled = true;
            }

            var a = new SimpleAdapter(ValidateDelete);
            var c = new TurnContext(a, TestMessage.Message());
            await c.DeleteActivityAsync("12345");
            Assert.IsTrue(deleteCalled);
        }

        [TestMethod]
        public async Task DeleteConversationReferenceToAdapter()
        {
            bool deleteCalled = false;

            void ValidateDelete(ConversationReference r)
            {
                Assert.IsNotNull(r);
                Assert.IsTrue(r.ActivityId == "12345");
                deleteCalled = true;
            }

            var a = new SimpleAdapter(ValidateDelete);
            var c = new TurnContext(a, TestMessage.Message());

            var reference = new ConversationReference("12345");

            await c.DeleteActivityAsync(reference);
            Assert.IsTrue(deleteCalled);
        }

        [TestMethod]
        public async Task InterceptOnDelete()
        {
            bool adapterCalled = false;

            void ValidateDelete(ConversationReference r)
            {
                adapterCalled = true;
                Assert.Fail("Should not be called.");
            }

            var a = new SimpleAdapter(ValidateDelete);
            var c = new TurnContext(a, new Activity());

            bool wasCalled = false;
            c.OnDeleteActivity((context, convRef, next) =>
            {
                Assert.IsNotNull(convRef, "Null activity passed in");
                wasCalled = true;

                // Do Not Call Next
                return Task.FromResult<ResourceResponse[]>(null);
            });

            await c.DeleteActivityAsync("1234");
            Assert.IsTrue(wasCalled); // Interceptor was called
            Assert.IsFalse(adapterCalled); // Adapter was not
        }

        [TestMethod]
        public async Task InterceptAndMutateOnDelete()
        {
            bool adapterCalled = false;

            void ValidateDelete(ConversationReference r)
            {
                Assert.IsTrue(r.ActivityId == "mutated");
                adapterCalled = true;
            }

            var a = new SimpleAdapter(ValidateDelete);
            var c = new TurnContext(a, new Activity());

            c.OnDeleteActivity(async (context, convRef, next) =>
            {
                Assert.IsNotNull(convRef, "Null activity passed in");
                Assert.IsTrue(convRef.ActivityId == "1234", "Incorrect Activity Id");
                convRef.ActivityId = "mutated";
                await next();
            });

            await c.DeleteActivityAsync("1234");
            Assert.IsTrue(adapterCalled); // Adapter was called + valided the change
        }

        [TestMethod]
        public async Task ThrowExceptionInOnSend()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());

            c.OnSendActivities((context, activities, next) =>
            {
                throw new Exception("test");
            });

            try
            {
                await c.SendActivityAsync(TestMessage.Message());
                Assert.Fail("Should not get here");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message == "test");
            }
        }

        public async Task MyBotLogic(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.AsMessageActivity().Text)
            {
                case "count":
                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("one"));
                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("two"));
                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("three"));
                    break;
                case "ignore":
                    break;
                case "TestResponded":
                    if (turnContext.Responded == true)
                    {
                        throw new InvalidOperationException("Responded Is True");
                    }

                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("one"));

                    if (turnContext.Responded == false)
                    {
                        throw new InvalidOperationException("Responded Is True");
                    }

                    break;
                default:
                    await turnContext.SendActivityAsync(
                        turnContext.Activity.CreateReply($"echo:{turnContext.Activity.Text}"));
                    break;
            }
        }
    }
}
