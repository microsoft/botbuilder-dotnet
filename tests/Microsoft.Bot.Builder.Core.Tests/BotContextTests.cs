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
        public class SimpleAdapter : BotAdapter
        {
            public async override Task DeleteActivity(ConversationReference reference)
            {
                Assert.IsNotNull(reference, "SimpleAdapter.deleteActivity: missing reference");
                Assert.IsTrue(reference.ActivityId == "1234", $"SimpleAdapter.deleteActivity: invalid activityId of {reference.ActivityId}");
            }

            public async override Task SendActivity(params Activity[] activities)
            {
                Assert.IsNotNull(activities, "SimpleAdapter.deleteActivity: missing reference");
                Assert.IsTrue(activities.Count() > 0, "SimpleAdapter.sendActivities: empty activities array.");
            }

            public async override Task<ResourceResponse> UpdateActivity(Activity activity)
            {
                Assert.IsNotNull(activity, "SimpleAdapter.updateActivity: missing activity");
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
