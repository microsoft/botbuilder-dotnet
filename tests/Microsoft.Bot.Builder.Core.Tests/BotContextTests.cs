using System;
using System.Linq;
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
            BotContext c = new BotContext(new TestAdapter(), new Activity())
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
            BotContext c = new BotContext(new SimpleAdapter(), TestMessage.Message());
            Assert.IsTrue(c.Request.Id == "1234");
        }

        [TestMethod]
        public async Task SendAndSetResponded()
        {
            SimpleAdapter a = new SimpleAdapter();
            BotContext c = new BotContext(a, new Activity());
            Assert.IsFalse(c.Responded);
            await c.SendActivity(TestMessage.Message());
            Assert.IsTrue(c.Responded);            
        }

        [TestMethod]
        public async Task SendAndSetRespondedUsingIMessageActivity()
        {
            SimpleAdapter a = new SimpleAdapter();
            BotContext c = new BotContext(a, new Activity());
            Assert.IsFalse(c.Responded);

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
            BotContext c = new BotContext(a, new Activity());            
            await c.SendActivity(TestMessage.Message());
            Assert.IsTrue(foundActivity);
        }

        [TestMethod]
        public async Task CallOnSendBeforeDelivery()
        {         
            SimpleAdapter a = new SimpleAdapter();
            BotContext c = new BotContext(a, new Activity());

            int count = 0;
            c.OnSendActivity(async (context, activities, next) =>
            {               
               Assert.IsNotNull(activities, "Null Array passed in");
               count = activities.Count();
               await next(); 
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
            BotContext c = new BotContext(a, new Activity());
          
            int count = 0;
            c.OnSendActivity(async (context, activities, next) =>
            {
                Assert.IsNotNull(activities, "Null Array passed in");
                count = activities.Count();
                // Do not call next. 
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
            BotContext c = new BotContext(a, new Activity());
            
            c.OnSendActivity(async (context, activities, next) =>
            {
                Assert.IsNotNull(activities, "Null Array passed in");
                Assert.IsTrue(activities.Count() == 1);
                Assert.IsTrue(activities[0].Id == "1234", "Unknown Id Passed In");
                activities[0].Id = "changed";
                await next(); 
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
                Assert.IsTrue(activity.Id == "1234");
                foundActivity = true;
            }

            SimpleAdapter a = new SimpleAdapter(ValidateUpdate);
            BotContext c = new BotContext(a, new Activity());
            await c.UpdateActivity(TestMessage.Message());
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
            c.OnUpdateActivity(async (context, activity, next) =>
            {
                Assert.IsNotNull(activity, "Null activity passed in");
                wasCalled = true;
                await next();
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
            BotContext c = new BotContext(a, new Activity());

            bool wasCalled = false;
            c.OnUpdateActivity(async (context, activity, next) =>
            {
                Assert.IsNotNull(activity, "Null activity passed in");
                wasCalled = true;
                // Do Not Call Next
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
            BotContext c = new BotContext(a, new Activity());

            c.OnUpdateActivity(async (context, activity, next) =>
            {
                Assert.IsNotNull(activity, "Null activity passed in");
                Assert.IsTrue(activity.Id == "1234");
                activity.Id = "mutated";
                await next(); 
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
            BotContext c = new BotContext(a, TestMessage.Message()); 
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
            BotContext c = new BotContext(a, new Activity());
            
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
            BotContext c = new BotContext(a, new Activity());
            
            c.OnSendActivity(async (context, activities, next) =>
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

