// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Ai.QnA.Tests
{
    [TestClass]
    public class QnaMakerMiddlewareTests
    {
        public string knowlegeBaseId = TestUtilities.GetKey("QNAKNOWLEDGEBASEID");
        public string subscriptionKey = TestUtilities.GetKey("QNASUBSCRIPTIONKEY");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestMiddleware()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }

            TestAdapter adapter = new TestAdapter()
                .Use(new QnAMakerMiddleware(new QnAMakerMiddlewareOptions()
                {
                    KnowledgeBaseId = knowlegeBaseId,
                    SubscriptionKey = subscriptionKey,
                    Top = 1
                }));


            await new TestFlow(adapter, async (context) =>
                {
                    if (context.Activity.Text == "foo")
                    {
                        await context.SendActivity(context.Activity.Text);
                    }                    
                })
                .Send("foo")
                    .AssertReply("foo", "passthrough")
                .Send("how do I clean the stove?")
                    .AssertReply("BaseCamp: You can use a damp rag to clean around the Power Pack. Do not attempt to detach it from the stove body. As with any electronic device, never pour water on it directly. CampStove 2 &amp; CookStove: Power module: Remove the plastic power module from the fuel chamber and wipe it down with a damp cloth with soap and water. DO NOT submerge the power module in water or get it excessively wet. Fuel chamber: Wipe out with a nylon brush as needed. The pot stand at the top of the fuel chamber can be wiped off with a damp cloth and dried well. The fuel chamber can also be washed in a dishwasher. Dry very thoroughly.")
                .StartTest();
        }

        private bool EnvironmentVariablesDefined()
        {
            return knowlegeBaseId != null && subscriptionKey != null;
        }
    }
}
