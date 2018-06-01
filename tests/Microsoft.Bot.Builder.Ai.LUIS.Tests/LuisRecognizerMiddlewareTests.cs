// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Bot.Schema;
using Microsoft.Cognitive.LUIS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Ai.LUIS.Tests
{
    [TestClass]
    public class LuisRecognizerMiddlewareTests
    {
        private readonly string _luisAppId = TestUtilities.GetKey("LUISAPPID");
        private readonly string _subscriptionKey = TestUtilities.GetKey("LUISAPPKEY");
        private readonly string _luisUriBase = TestUtilities.GetKey("LUISURIBASE");

        [TestMethod]
        public void LuisRecognizer_MiddlewareConstruction()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var middleware = GetLuisRecognizerMiddleware();
            Assert.IsNotNull(middleware);
            Assert.ThrowsException<ArgumentNullException>(() => new LuisRecognizerMiddleware(null));
        }

        [TestMethod]
        public async Task LuisRecognizer_TestMiddleware()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var adapter = new TestAdapter(sendTraceActivity: true)
                .Use(GetLuisRecognizerMiddleware(true));

            const string utterance = @"My name is Emad";
            const string botResponse = @"Hi Emad";
            await new TestFlow(adapter, async context =>
                {
                    if (context.Activity.Text == utterance)
                    {
                        await context.SendActivity(botResponse);
                    }                    
                })
                .Test(utterance, activity =>
                {
                    var traceActivity = activity as ITraceActivity;
                    Assert.IsNotNull(traceActivity);
                    Assert.AreEqual(LuisRecognizerMiddleware.LuisTraceType, traceActivity.ValueType);
                    Assert.AreEqual(LuisRecognizerMiddleware.LuisTraceLabel, traceActivity.Label);

                    var luisTraceInfo = traceActivity.Value as LuisTraceInfo;
                    Assert.IsNotNull(luisTraceInfo);
                    Assert.IsNotNull(luisTraceInfo.RecognizerResult);
                    Assert.IsNotNull(luisTraceInfo.LuisModel);
                    Assert.IsNotNull(luisTraceInfo.LuisOptions);
                    Assert.IsNotNull(luisTraceInfo.LuisModel);

                    Assert.AreEqual(luisTraceInfo.RecognizerResult.Text, utterance);
                    Assert.IsNotNull(luisTraceInfo.RecognizerResult.Intents["SpecifyName"]);
                    Assert.AreEqual(luisTraceInfo.LuisResult.Query, utterance);
                    Assert.AreEqual(luisTraceInfo.LuisModel.ModelID, _luisAppId);
                    Assert.AreEqual(luisTraceInfo.LuisOptions.Verbose, true);

                }, "luisTraceInfo")
                .Send(utterance)
                .AssertReply(botResponse, "passthrough")
                .StartTest();
        }

        [TestMethod]
        public void LuisRecognizer_ObfuscateSensitiveData()
        {
            var model = new LuisModel(Guid.NewGuid().ToString(), "abc", new Uri("http://luis.ai"));
            var obfuscated = LuisRecognizerMiddleware.RemoveSensitiveData(model);

            Assert.AreEqual(LuisRecognizerMiddleware.Obfuscated, obfuscated.SubscriptionKey);
            Assert.AreEqual(model.ApiVersion, obfuscated.ApiVersion);
            Assert.AreEqual(model.ModelID, obfuscated.ModelID);
            Assert.AreEqual(model.Threshold, obfuscated.Threshold);
            Assert.AreEqual(model.UriBase.Host, obfuscated.UriBase.Host);

        }

        [TestMethod]
        public async Task LuisRecognizer_MiddlewareNullUtterance()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            await RunTest(null);
            await RunTest(string.Empty);
            await RunTest(" ");
        }

        private async Task RunTest(string utterance)
        {
            var adapter = new TestAdapter()
                .Use(GetLuisRecognizerMiddleware(true));

            var messageActivity = Activity.CreateMessageActivity();
            messageActivity.Text = utterance;

            const string botResponse = @"Hi";
            await new TestFlow(adapter, async context =>
                {
                    if (context.Activity.Text == utterance)
                    {
                        await context.SendActivity(botResponse);
                    }
                })
                .Send(messageActivity)
                .AssertReply(botResponse, "passthrough")
                .StartTest();
        }

        private LuisRecognizerMiddleware GetLuisRecognizerMiddleware(bool verbose = false, ILuisOptions luisOptions = null)
        {
            var luisRecognizerOptions = new LuisRecognizerOptions { Verbose = verbose };
            var luisModel = new LuisModel(_luisAppId, _subscriptionKey, new Uri(_luisUriBase));
            return new LuisRecognizerMiddleware(luisModel, luisRecognizerOptions, luisOptions ?? new LuisRequest{Verbose = verbose});
        }

        private bool EnvironmentVariablesDefined()
        {
            return _luisAppId != null && _subscriptionKey != null && _luisUriBase != null;
        }
    }
}
