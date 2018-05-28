using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    public class TurnContextTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ConstructorNullAdapter()
        {
            TurnContext c = new TurnContext(null, new Activity());
            Assert.Fail("Should Fail due to null Adapter");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ConstructorNullActivity()
        {
            TestAdapter a = new TestAdapter();
            TurnContext c = new TurnContext(a, null);
            Assert.Fail("Should Fail due to null Activty");
        }
        [TestMethod]
        public async Task Constructor()
        {
            TurnContext c = new TurnContext(new TestAdapter(), new Activity());
            Assert.IsNotNull(c);
        }

        [TestMethod]
        public async Task RespondedIsFalse()
        {
            TurnContext c = new TurnContext(new TestAdapter(), new Activity());
            Assert.IsFalse(c.Responded);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UnableToSetRespondedToFalse()
        {
            TurnContext c = new TurnContext(new TestAdapter(), new Activity())
            {
                Responded = false // should throw
            };
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
        public async Task GetThrowsOnNullKey()
        {
            TurnContext c = new TurnContext(new SimpleAdapter(), new Activity());
            c.Services.Get<object>(null);
        }

        [TestMethod]
        public async Task GetReturnsNullOnEmptyKey()
        {
            TurnContext c = new TurnContext(new SimpleAdapter(), new Activity());
            object service = c.Services.Get<object>(string.Empty); // empty key
            Assert.IsNull(service, "Should not have found a service under an empty key");
        }


        [TestMethod]
        public async Task GetReturnsNullWithUnknownKey()
        {
            TurnContext c = new TurnContext(new SimpleAdapter(), new Activity());
            object o = c.Services.Get<object>("test");
            Assert.IsNull(o);
        }

        [TestMethod]
        public async Task CacheValueUsingGetAndSet()
        {
            TurnContext c = new TurnContext(new SimpleAdapter(), new Activity());

            c.Services.Add("bar", "foo");
            var result = c.Services.Get<string>("bar");

            Assert.AreEqual("foo", result);
        }
        [TestMethod]
        public async Task CacheValueUsingGetAndSetGenericWithTypeAsKeyName()
        {
            TurnContext c = new TurnContext(new SimpleAdapter(), new Activity());

            c.Services.Add<string>("foo");
            string result = c.Services.Get<string>();

            Assert.AreEqual("foo", result);
        }

        [TestMethod]
        public async Task RequestIsSet()
        {
            TurnContext c = new TurnContext(new SimpleAdapter(), TestMessage.Message());
            Assert.IsTrue(c.Activity.Id == "1234");
        }

        [TestMethod]
        public async Task SendAndSetResponded()
        {
            SimpleAdapter a = new SimpleAdapter();
            TurnContext c = new TurnContext(a, new Activity());
            Assert.IsFalse(c.Responded);            
            var response = await c.SendActivity(TestMessage.Message("testtest"));

            Assert.IsTrue(c.Responded);
            Assert.IsTrue(response.Id == "testtest");
        }

        [TestMethod]
        public async Task SendBatchOfActivities()
        {
            SimpleAdapter a = new SimpleAdapter();
            TurnContext c = new TurnContext(a, new Activity());
            Assert.IsFalse(c.Responded);

            var message1 = TestMessage.Message("message1");
            var message2 = TestMessage.Message("message2");

            var response = await c.SendActivities(new IActivity[] { message1, message2 } );

            Assert.IsTrue(c.Responded);
            Assert.IsTrue(response.Length == 2);
            Assert.IsTrue(response[0].Id == "message1");
            Assert.IsTrue(response[1].Id == "message2");
        }

        [TestMethod]
        public async Task SendAndSetRespondedUsingIMessageActivity()
        {
            SimpleAdapter a = new SimpleAdapter();
            TurnContext c = new TurnContext(a, new Activity());
            Assert.IsFalse(c.Responded);

            IMessageActivity msg = TestMessage.Message().AsMessageActivity();
            await c.SendActivity(msg);
            Assert.IsTrue(c.Responded);
        }

        [TestMethod]
        public async Task TraceActivitiesDoNoSetResponded()
        {
            SimpleAdapter a = new SimpleAdapter();
            TurnContext c = new TurnContext(a, new Activity());
            Assert.IsFalse(c.Responded);

            // Send a Trace Activity, and make sure responded is NOT set. 
            ITraceActivity trace  = Activity.CreateTraceActivity("trace");            
            await c.SendActivity(trace);
            Assert.IsFalse(c.Responded);

            // Just to sanity check everything, send a Message and verify the 
            // responded flag IS set. 
            IMessageActivity msg = TestMessage.Message().AsMessageActivity();
            await c.SendActivity(msg);
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
            TurnContext c = new TurnContext(a, new Activity());            
            await c.SendActivity(TestMessage.Message());
            Assert.IsTrue(foundActivity);
        }

        [TestMethod]
        public async Task CallOnSendBeforeDelivery()
        {         
            SimpleAdapter a = new SimpleAdapter();
            TurnContext c = new TurnContext(a, new Activity());

            int count = 0;
            c.OnSendActivities(async (context, activities, next) =>
            {               
               Assert.IsNotNull(activities, "Null Array passed in");
               count = activities.Count();
               return await next(); 
            });

            await c.SendActivity(TestMessage.Message());

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
            TurnContext c = new TurnContext(a, new Activity());
          
            int count = 0;
            c.OnSendActivities(async (context, activities, next) =>
            {
                Assert.IsNotNull(activities, "Null Array passed in");
                count = activities.Count();
                // Do not call next. 
                return null;
            });

            await c.SendActivity(TestMessage.Message());

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
            TurnContext c = new TurnContext(a, new Activity());
            
            c.OnSendActivities(async (context, activities, next) =>
            {
                Assert.IsNotNull(activities, "Null Array passed in");
                Assert.IsTrue(activities.Count() == 1);
                Assert.IsTrue(activities[0].Id == "1234", "Unknown Id Passed In");
                activities[0].Id = "changed";
                return await next(); 
            });

            await c.SendActivity(TestMessage.Message());

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

            SimpleAdapter a = new SimpleAdapter(ValidateUpdate);
            TurnContext c = new TurnContext(a, new Activity());
            
            var message = TestMessage.Message("test");            
            var updateResult = await c.UpdateActivity(message);

            Assert.IsTrue(foundActivity);
            Assert.IsTrue(updateResult.Id == "test");
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
            await c.UpdateActivity(TestMessage.Message());
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
            TurnContext c = new TurnContext(a, new Activity());

            bool wasCalled = false;
            c.OnUpdateActivity(async (context, activity, next) =>
            {
                Assert.IsNotNull(activity, "Null activity passed in");
                wasCalled = true;
                // Do Not Call Next
                return null;
            });

            await c.UpdateActivity(TestMessage.Message());
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
            TurnContext c = new TurnContext(a, new Activity());

            c.OnUpdateActivity(async (context, activity, next) =>
            {
                Assert.IsNotNull(activity, "Null activity passed in");
                Assert.IsTrue(activity.Id == "1234");
                activity.Id = "mutated";
                return await next(); 
            });

            await c.UpdateActivity(TestMessage.Message());
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
            TurnContext c = new TurnContext(a, TestMessage.Message()); 
            await c.DeleteActivity("12345"); 
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

            SimpleAdapter a = new SimpleAdapter(ValidateDelete);
            TurnContext c = new TurnContext(a, TestMessage.Message());

            var reference = new ConversationReference("12345");

            await c.DeleteActivity(reference);
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
            TurnContext c = new TurnContext(a, new Activity());

            bool wasCalled = false;
            c.OnDeleteActivity(async (context, convRef, next) =>
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
            TurnContext c = new TurnContext(a, new Activity());
            
            c.OnDeleteActivity(async (context, convRef, next) =>
            {
                Assert.IsNotNull(convRef, "Null activity passed in");
                Assert.IsTrue(convRef.ActivityId == "1234", "Incorrect Activity Id");
                convRef.ActivityId = "mutated";
                await next();
            });

            await c.DeleteActivity("1234");
            Assert.IsTrue(adapterCalled); // Adapter was called + valided the change
        }

        [TestMethod]
        public async Task ThrowExceptionInOnSend()
        {
            SimpleAdapter a = new SimpleAdapter();
            TurnContext c = new TurnContext(a, new Activity());
            
            c.OnSendActivities(async (context, activities, next) =>
            {
                throw new Exception("test");                 
            });

            try
            {
                await c.SendActivity(TestMessage.Message());
                Assert.Fail("Should not get here");
            }
            catch(Exception ex)
            {
                Assert.IsTrue(ex.Message == "test");
            }            
        }        

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
                case "TestResponded":
                    if (context.Responded == true)
                        throw new InvalidOperationException("Responded Is True");

                    await context.SendActivity(context.Activity.CreateReply("one"));

                    if (context.Responded == false)
                        throw new InvalidOperationException("Responded Is True");
                    break;
                default:
                    await context.SendActivity(
                        context.Activity.CreateReply($"echo:{context.Activity.Text}"));
                    break;
            }
        }
    }
}

