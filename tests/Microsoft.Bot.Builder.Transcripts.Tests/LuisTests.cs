// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Transcripts.Tests
{
    [TestClass]
    public class LuisTests
    {
        private readonly string _luisAppId = TestUtilities.GetKey("LUISAPPID_TRANSCRIPT");
        private readonly string _subscriptionKey = TestUtilities.GetKey("LUISAPPKEY_TRANSCRIPT");
        private readonly string _luisUriBase = TestUtilities.GetKey("LUISURIBASE_TRANSCRIPT");

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task LuisMiddleware()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

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
