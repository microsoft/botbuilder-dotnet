// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("BotAdapter")]
    public class BotAdapterTests
    {
        [TestMethod]        
        public void AdapterSingleUse()
        {
            var a = new SimpleAdapter();
            a.Use(new CallCountingMiddleware()); 
        }

        [TestMethod]
        public void AdapterUseChaining()
        {
            var a = new SimpleAdapter();
            a.Use(new CallCountingMiddleware()).Use(new CallCountingMiddleware());
        }

        [TestMethod]
        public async Task PassResourceResponsesThrough()
        {
            void ValidateResponses(Activity[] activities)
            {
                // no need to do anything. 
            }

            var a = new SimpleAdapter(ValidateResponses);
            var c = new TurnContext(a, new Activity());

            var activityId = Guid.NewGuid().ToString();
            var activity = TestMessage.Message();
            activity.Id = activityId;

            var resourceResponse = await c.SendActivityAsync(activity);
            Assert.IsTrue(resourceResponse.Id == activityId, "Incorrect response Id returned"); 
        }
    }

    public class CallCountingMiddleware : IMiddleware
    {
        public int Calls { get; set; }
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            Calls++;
            await next(cancellationToken);
        }
    }
}
