using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai.QnA;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Transcripts.Tests
{
    [TestClass]
    public class QnATests
    {
        private readonly string knowlegeBaseId = TestUtilities.GetKey("QNAKNOWLEDGEBASEID");
        private readonly string endpointKey = TestUtilities.GetKey("QNAENDPOINTKEY");
        private readonly string hostname = TestUtilities.GetKey("QNAHOSTNAME");
        
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task QnAMiddleware()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }

            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(GetQnAMiddleware());

            var flow = new TestFlow(adapter, async (context) => {
                if (!context.Responded)
                {
                    await context.SendActivity("default message");
                }
            });

            await flow.Test(activities, (expected, actual) => {
                Assert.AreEqual(expected.Type, actual.Type);
                var expectedMessage = expected.AsMessageActivity();
                var actualMessage = actual.AsMessageActivity();
                if (expectedMessage != null)
                {
                    Assert.AreEqual(expectedMessage.Text, actualMessage.Text);
                }
            }).StartTest();
        }

        private QnAMakerMiddleware GetQnAMiddleware()
        {
            return new QnAMakerMiddleware(new QnAMakerEndpoint {
                KnowledgeBaseId = knowlegeBaseId,
                EndpointKey = endpointKey,
                Host = hostname
            });
        }

        private bool EnvironmentVariablesDefined()
        {
            return knowlegeBaseId != null && endpointKey != null && hostname != null;
        }
    }
}
