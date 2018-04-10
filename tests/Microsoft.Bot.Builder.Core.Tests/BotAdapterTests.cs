using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Tests
{
    [TestClass]
    [TestCategory("BotAdapter")]
    public class BotAdapterTests
    {
        [TestMethod]        
        public async Task AdapterSingleUse()
        {
            SimpleAdapter a = new SimpleAdapter();
            a.Use(new CallCountingMiddleware()); 

            // Compiled. Test passed. 
        }

        [TestMethod]
        public async Task AdapterUseChaining()
        {
            SimpleAdapter a = new SimpleAdapter();
            a.Use(new CallCountingMiddleware()).Use(new CallCountingMiddleware());
            // Compiled. Test passed. 
        }

        [TestMethod]
        public async Task PassResourceResponsesThrough()
        {
            void ValidateResponses(Activity[] activities)
            {
                // no need to do anything. 
            }

            SimpleAdapter a = new SimpleAdapter(ValidateResponses);
            TurnContext c = new TurnContext(a, new Activity());

            string activityId = Guid.NewGuid().ToString();
            var activity = TestMessage.Message();
            activity.Id = activityId;

            var resourceResponse = await c.SendActivity(activity);
            Assert.IsTrue(resourceResponse.Id == activityId, "Incorrect response Id returned"); 
        }
    }

    public class CallCountingMiddleware : IMiddleware
    {
        public int Calls { get; set; }
        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            Calls++;
            await next();
        }

    }
}
