// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Ai.QnA.Tests
{
    [TestClass]
    public class QnaMakerMiddlewareTests
    {
        public string knowlegeBaseId = TestUtilities.GetKey("QNAKNOWLEDGEBASEID");
        public string endpointKey = TestUtilities.GetKey("QNAENDPOINTKEY");
        public readonly string hostname = TestUtilities.GetKey("QNAHOSTNAME");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestMiddleware_NoResults()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }
            const string passUtterance = @"Foo";
            TestAdapter adapter = new TestAdapter()
                .Use(new QnAMakerMiddleware(
                    new QnAMakerEndpoint
                    {
                        KnowledgeBaseId = knowlegeBaseId,
                        EndpointKey = endpointKey,
                        Host = hostname
                    },
                    new QnAMakerMiddlewareOptions
                    {
                        Top = 1
                    }));

            await new TestFlow(adapter, async (context) =>
            {
                await context.SendActivity(context.Activity.Text);
            })
                .Test(passUtterance, activity =>
                {
                    var traceActivity = activity as ITraceActivity;
                    Assert.IsNotNull(traceActivity);
                    Assert.AreEqual(QnAMakerMiddleware.QnAMakerTraceType, traceActivity.ValueType);
                    Assert.AreEqual(QnAMakerMiddleware.QnAMakerTraceLabel, traceActivity.Label);

                    var qnaMakerTraceInfo = traceActivity.Value as QnAMakerTraceInfo;
                    Assert.IsNotNull(qnaMakerTraceInfo);
                    Assert.IsNotNull(qnaMakerTraceInfo.Message);
                    Assert.IsNotNull(qnaMakerTraceInfo.QueryResults);
                    Assert.IsNotNull(qnaMakerTraceInfo.KnowledgeBaseId);
                    Assert.IsNotNull(qnaMakerTraceInfo.ScoreThreshold);
                    Assert.IsNotNull(qnaMakerTraceInfo.Top);
                    Assert.IsNotNull(qnaMakerTraceInfo.StrictFilters);
                    Assert.IsNotNull(qnaMakerTraceInfo.MetadataBoost);

                    Assert.AreEqual(qnaMakerTraceInfo.Message.Text, passUtterance);
                    Assert.AreEqual(qnaMakerTraceInfo.QueryResults.Length, 0);
                }, "qnaMakerTraceInfo")
                .Send(passUtterance)
                    .AssertReply(passUtterance, "passthrough")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestMiddleware_OneResult()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }
            const string goodUtterance = @"how do I clean the stove?";
            const string botResponse = @"BaseCamp: You can use a damp rag to clean around the Power Pack. Do not attempt to detach it from the stove body. As with any electronic device, never pour water on it directly. CampStove 2 & CookStove: Power module: Remove the plastic power module from the fuel chamber and wipe it down with a damp cloth with soap and water. DO NOT submerge the power module in water or get it excessively wet. Fuel chamber: Wipe out with a nylon brush as needed. The pot stand at the top of the fuel chamber can be wiped off with a damp cloth and dried well. The fuel chamber can also be washed in a dishwasher. Dry very thoroughly.";
            TestAdapter adapter = new TestAdapter()
                .Use(new QnAMakerMiddleware(
                    new QnAMakerEndpoint
                    {
                        KnowledgeBaseId = knowlegeBaseId,
                        EndpointKey = endpointKey,
                        Host = hostname
                    },
                    new QnAMakerMiddlewareOptions
                    {
                        Top = 1
                    }));

            await new TestFlow(adapter, async (context) =>
            {
                if (context.Activity.Text == "foo")
                {
                    await context.SendActivity(context.Activity.Text);
                }
            })
                .Test(goodUtterance, activity =>
                {
                    var traceActivity = activity as ITraceActivity;
                    Assert.IsNotNull(traceActivity);
                    Assert.AreEqual(QnAMakerMiddleware.QnAMakerTraceType, traceActivity.ValueType);
                    Assert.AreEqual(QnAMakerMiddleware.QnAMakerTraceLabel, traceActivity.Label);

                    var qnaMakerTraceInfo = traceActivity.Value as QnAMakerTraceInfo;
                    Assert.IsNotNull(qnaMakerTraceInfo);
                    Assert.IsNotNull(qnaMakerTraceInfo.Message);
                    Assert.IsNotNull(qnaMakerTraceInfo.QueryResults);
                    Assert.IsNotNull(qnaMakerTraceInfo.KnowledgeBaseId);
                    Assert.IsNotNull(qnaMakerTraceInfo.ScoreThreshold);
                    Assert.IsNotNull(qnaMakerTraceInfo.Top);
                    Assert.IsNotNull(qnaMakerTraceInfo.StrictFilters);
                    Assert.IsNotNull(qnaMakerTraceInfo.MetadataBoost);

                    Assert.AreEqual(qnaMakerTraceInfo.Message.Text, goodUtterance);
                    Assert.AreEqual(qnaMakerTraceInfo.QueryResults.Length, 1);
                    Assert.AreEqual(qnaMakerTraceInfo.QueryResults[0].Answer, botResponse);
                    Assert.AreEqual(qnaMakerTraceInfo.KnowledgeBaseId, knowlegeBaseId);
                }, "qnaMakerTraceInfo")
                .Send(goodUtterance)
                    .AssertReply(botResponse)
                .StartTest();
        }

        private bool EnvironmentVariablesDefined()
        {
            return knowlegeBaseId != null && endpointKey != null && hostname != null;
        }
    }
}
