// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Cognitive.LUIS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.LUIS.Tests
{
    [TestClass]
    public class LuisRecognizerMiddlewareTests
    {
        private readonly string _luisAppId = TestUtilities.GetKey("LUISAPPID");
        private readonly string _subscriptionKey = TestUtilities.GetKey("LUISAPPKEY");
        private readonly string _luisUriBase = TestUtilities.GetKey("LUISURIBASE");

        [TestMethod]
        public async Task LuisRecognizer_TestMiddleware()
        {
            var adapter = new TestAdapter()
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
                    var eventActivity = activity.AsEventActivity();
                    Assert.AreEqual(LuisRecognizerMiddleware.LuisTraceEventName, eventActivity.Name);

                    var luisTraceInfo = eventActivity.Value as LuisTraceInfo;
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

        private LuisRecognizerMiddleware GetLuisRecognizerMiddleware(bool verbose = false, ILuisOptions luisOptions = null)
        {
            var luisRecognizerOptions = new LuisRecognizerOptions { Verbose = verbose };
            var luisModel = new LuisModel(_luisAppId, _subscriptionKey, new Uri(_luisUriBase));
            return new LuisRecognizerMiddleware(luisModel, luisRecognizerOptions, luisOptions ?? new LuisRequest{Verbose = verbose});
        }

    }
}
