// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Luis.Tests
{
    [TestClass]
    public class LuisTraceInfoTests
    {
        [TestMethod]
        public void LuisTraceInfo_Serialization()
        {
            var luisTraceInfo = new LuisTraceInfo
            {
                Application = new LuisApplication(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "Westus"),
                Options = new LuisPredictionOptions { Verbose = true },
                LuisResult = new LuisResult { Query = "hi" },
                RecognizerResult = new RecognizerResult { Text = "hi" },
            };

            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var serialized = JsonConvert.SerializeObject(luisTraceInfo, serializerSettings);
            var deserialized = JsonConvert.DeserializeObject<LuisTraceInfo>(serialized, serializerSettings);

            Assert.IsNotNull(deserialized);
            Assert.IsNotNull(deserialized.Application);
            Assert.IsNotNull(deserialized.Options);
            Assert.IsNotNull(deserialized.LuisResult);
            Assert.IsNotNull(deserialized.RecognizerResult);
            Assert.AreEqual(luisTraceInfo.Application.SubscriptionKey, deserialized.Application.SubscriptionKey);
            Assert.AreEqual(luisTraceInfo.Options.Verbose, deserialized.Options.Verbose);
            Assert.AreEqual(luisTraceInfo.LuisResult.Query, deserialized.LuisResult.Query);
            Assert.AreEqual(luisTraceInfo.RecognizerResult.Text, deserialized.RecognizerResult.Text);
        }
    }
}
