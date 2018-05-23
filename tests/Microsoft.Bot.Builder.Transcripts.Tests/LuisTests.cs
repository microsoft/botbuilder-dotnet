using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Transcripts.Tests
{
    [TestClass]
    public class LuisTests
    {
        private readonly string _luisAppId = TestUtilities.GetKey("LUISAPPID");
        private readonly string _subscriptionKey = TestUtilities.GetKey("LUISAPPKEY");
        private readonly string _luisUriBase = TestUtilities.GetKey("LUISURIBASE");

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task LuisMiddleware()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(GetLuisMiddleware());

            var flow = new TestFlow(adapter, async (context) => {
                var luisResult = context.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);
                var intent = luisResult.GetTopScoringIntent().intent;
                if (intent != "None")
                {
                    await context.SendActivity($"intent:{intent}");
                }
                else
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

        private LuisRecognizerMiddleware GetLuisMiddleware()
        {
            var model = new LuisModel(_luisAppId, _subscriptionKey, new Uri(_luisUriBase));
            return new LuisRecognizerMiddleware(model);
        }

        private bool EnvironmentVariablesDefined()
        {
            return _luisAppId != null && _subscriptionKey != null && _luisUriBase != null;
        }
    }
}
