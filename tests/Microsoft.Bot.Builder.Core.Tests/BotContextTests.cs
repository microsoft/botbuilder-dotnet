using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    public class BotContextTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ConstructorNullAdapter()
        {
            BotContext c = new BotContext(null, new Activity());
            Assert.Fail("Should Fail due to null Adapter");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ConstructorNullActivity()
        {
            TestAdapter a = new TestAdapter();
            BotContext c = new BotContext(a, null);
            Assert.Fail("Should Fail due to null Activty");
        }
        [TestMethod]
        public async Task Constructor()
        {
            BotContext c = new BotContext(new TestAdapter(), new Activity());
            Assert.IsNotNull(c);
        }

        [TestMethod]
        public async Task RespondedIsFalse()
        {
            BotContext c = new BotContext(new TestAdapter(), new Activity());
            Assert.IsFalse(c.Responded);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UnableToSetRespondedToFalse()
        {
            BotContext c = new BotContext(new TestAdapter(), new Activity());
            c.Responded = false; // should throw
            Assert.Fail("Should have thrown");
        }

        [TestMethod]
        public async Task CacheValueUsingSetAndGet()
        {
            var adapter = new TestAdapter();
            await new TestFlow(adapter, MyBotLogic)
                    .Send("TestResponded")
                    .StartTest();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetThrowsOnEmptyKey()
        {
            BotContext c = new BotContext(new SimpleAdapter(), new Activity());
            c.Get(string.Empty); // empty key. Throw
            Assert.Fail("Did not throw");
        }


        [TestMethod]
        public async Task GetReturnsNullWithUnknownKey()
        {
            BotContext c = new BotContext(new SimpleAdapter(), new Activity());
            object o = c.Get("test");
            Assert.IsNull(o);
        }

        [TestMethod]
        public async Task CacheValueUsingGetAndSet()
        {
            BotContext c = new BotContext(new SimpleAdapter(), new Activity());

            c.Set("bar", "foo");
            var result = c.Get("bar");

            Assert.AreEqual("foo", result);
        }
        [TestMethod]
        public async Task CacheValueUsingGetAndSetGenericWithTypeAsKeyName()
        {
            BotContext c = new BotContext(new SimpleAdapter(), new Activity());

            c.Set<string>("foo");
            string result = c.Get<string>();

            Assert.AreEqual("foo", result);
        }

        [TestMethod]
        public async Task InspectKeyUsingHas()
        {
            BotContext c = new BotContext(new SimpleAdapter(), new Activity());

            Assert.IsFalse(c.Has("bar"), "Key should not exist");
            c.Set("bar", "foo");
            Assert.IsTrue(c.Has("bar"), "Key should exist");
        }

        [TestMethod]
        public async Task RequestIsSet()
        {
            BotContext c = new BotContext(new SimpleAdapter(), TestMessage());
            Assert.IsTrue(c.Request.Id == "1234");
        }

        [TestMethod]
        public async Task SendAndSetResponded()
        {
            SimpleAdapter a = new SimpleAdapter();
            BotContext c = new BotContext(a, new Activity());
            Assert.IsFalse(c.Responded);
            await c.SendActivity(TestMessage());
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

            SimpleAdapter a = new SimpleAdapter(ValidateResponses);
            BotContext c = new BotContext(a, new Activity());            
            await c.SendActivity(TestMessage());
            Assert.IsTrue(foundActivity);
        }

        [TestMethod]
        public async Task CallOnSendBeforeDelivery()
        {         
            SimpleAdapter a = new SimpleAdapter();
            BotContext c = new BotContext(a, new Activity());

            int count = 0;
            c.OnSendActivity(async (activities, next) =>
            {               
               Assert.IsNotNull(activities, "Null Array passed in");
               count = activities.Count();
               await next(); 
            });

            await c.SendActivity(TestMessage());

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

            SimpleAdapter a = new SimpleAdapter(ValidateResponses);
            BotContext c = new BotContext(a, new Activity());
          
            int count = 0;
            c.OnSendActivity(async (activities, next) =>
            {
                Assert.IsNotNull(activities, "Null Array passed in");
                count = activities.Count();
                // Do not call next. 
            });

            await c.SendActivity(TestMessage());

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

            SimpleAdapter a = new SimpleAdapter(ValidateResponses);
            BotContext c = new BotContext(a, new Activity());
            
            c.OnSendActivity(async (activities, next) =>
            {
                Assert.IsNotNull(activities, "Null Array passed in");
                Assert.IsTrue(activities.Count() == 1);
                Assert.IsTrue(activities[0].Id == "1234", "Unknown Id Passed In");
                activities[0].Id = "changed";
                await next(); 
            });

            await c.SendActivity(TestMessage());

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
                Assert.IsTrue(activity.Id == "1234");
                foundActivity = true;
            }

            SimpleAdapter a = new SimpleAdapter(ValidateUpdate);
            BotContext c = new BotContext(a, new Activity());
            await c.UpdateActivity(TestMessage());
            Assert.IsTrue(foundActivity);
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
            BotContext c = new BotContext(a, new Activity());

            bool wasCalled = false;
            c.OnUpdateActivity(async (activity, next) =>
            {
                Assert.IsNotNull(activity, "Null activity passed in");
                wasCalled = true;
                await next();
            });
            await c.UpdateActivity(TestMessage());
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

            SimpleAdapter a = new SimpleAdapter(ValidateUpdate);
            BotContext c = new BotContext(a, new Activity());

            bool wasCalled = false;
            c.OnUpdateActivity(async (activity, next) =>
            {
                Assert.IsNotNull(activity, "Null activity passed in");
                wasCalled = true;
                // Do Not Call Next
            });

            await c.UpdateActivity(TestMessage());
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

            SimpleAdapter a = new SimpleAdapter(ValidateUpdate);
            BotContext c = new BotContext(a, new Activity());

            c.OnUpdateActivity(async (activity, next) =>
            {
                Assert.IsNotNull(activity, "Null activity passed in");
                Assert.IsTrue(activity.Id == "1234");
                activity.Id = "mutated";
                await next(); 
            });

            await c.UpdateActivity(TestMessage());
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

            SimpleAdapter a = new SimpleAdapter(ValidateDelete);
            BotContext c = new BotContext(a, TestMessage()); 
            await c.DeleteActivity("12345"); 
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

            SimpleAdapter a = new SimpleAdapter(ValidateDelete);
            BotContext c = new BotContext(a, new Activity());

            bool wasCalled = false;
            c.OnDeleteActivity(async (convRef, next) =>
            {
                Assert.IsNotNull(convRef, "Null activity passed in");
                wasCalled = true;
                // Do Not Call Next
            });

            await c.DeleteActivity("1234"); 
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

            SimpleAdapter a = new SimpleAdapter(ValidateDelete);
            BotContext c = new BotContext(a, new Activity());
            
            c.OnDeleteActivity(async (convRef, next) =>
            {
                Assert.IsNotNull(convRef, "Null activity passed in");
                Assert.IsTrue(convRef.ActivityId == "1234", "Incorrect Activity Id");
                convRef.ActivityId = "mutated";
                await next();
            });

            await c.DeleteActivity("1234");
            Assert.IsTrue(adapterCalled); // Adapter was called + valided the change
        }


        private Activity TestMessage()
        {
            Activity a = new Activity
            {
                Type = ActivityTypes.Message,
                Id = "1234",
                Text = "test",
                From = new ChannelAccount()
                {
                    Id = "user",
                    Name = "User Name"
                },
                Recipient = new ChannelAccount()
                {
                    Id = "bot",
                    Name = "Bot Name"
                },
                Conversation = new ConversationAccount()
                {
                    Id = "convo",
                    Name = "Convo Name"
                },
                ChannelId = "UnitTest",
                ServiceUrl = "https://example.org"
            };
            return a;
        }

        public class SimpleAdapter : BotAdapter
        {
            private readonly Action<Activity[]> _callOnSend = null;
            private readonly Action<Activity> _callOnUpdate = null;
            private readonly Action<ConversationReference> _callOnDelete = null;

            public SimpleAdapter() { }
            public SimpleAdapter(Action<Activity[]> callOnSend) { _callOnSend = callOnSend; }
            public SimpleAdapter(Action<Activity> callOnUpdate) { _callOnUpdate = callOnUpdate; }
            public SimpleAdapter(Action<ConversationReference> callOnDelete) { _callOnDelete = callOnDelete; }

            public async override Task DeleteActivity(ConversationReference reference)
            {
                Assert.IsNotNull(reference, "SimpleAdapter.deleteActivity: missing reference");                
                _callOnDelete?.Invoke(reference);
            }

            public async override Task SendActivity(params Activity[] activities)
            {
                Assert.IsNotNull(activities, "SimpleAdapter.deleteActivity: missing reference");
                Assert.IsTrue(activities.Count() > 0, "SimpleAdapter.sendActivities: empty activities array.");

                _callOnSend?.Invoke(activities);
            }

            public async override Task<ResourceResponse> UpdateActivity(Activity activity)
            {
                Assert.IsNotNull(activity, "SimpleAdapter.updateActivity: missing activity");
                _callOnUpdate?.Invoke(activity); 
                return new ResourceResponse("testId");
            }
        }


        public async Task MyBotLogic(IBotContext context)
        {
            switch (context.Request.AsMessageActivity().Text)
            {
                case "count":
                    await context.SendActivity(context.Request.CreateReply("one"));
                    await context.SendActivity(context.Request.CreateReply("two"));
                    await context.SendActivity(context.Request.CreateReply("three"));
                    break;
                case "ignore":
                    break;
                case "TestResponded":
                    if (context.Responded == true)
                        throw new InvalidOperationException("Responded Is True");

                    await context.SendActivity(context.Request.CreateReply("one"));

                    if (context.Responded == false)
                        throw new InvalidOperationException("Responded Is True");
                    break;
                default:
                    await context.SendActivity(
                        context.Request.CreateReply($"echo:{context.Request.Text}"));
                    break;
            }
        }
    }
}

