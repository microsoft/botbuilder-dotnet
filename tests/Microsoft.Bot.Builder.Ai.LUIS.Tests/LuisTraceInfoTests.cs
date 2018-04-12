using System;
using Microsoft.Cognitive.LUIS;
using Microsoft.Cognitive.LUIS.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS.Tests
{
    [TestClass]
    public class LuisTraceInfoTests
    {
        [TestMethod]
        public void LuisTraceInfo_Serialization()
        {
            var luisTraceInfo = new LuisTraceInfo
            {
                LuisModel = new LuisModel(Guid.NewGuid().ToString(), "abc", new Uri("https://luis.ai")),
                LuisOptions = new LuisRequest {Verbose = true},
                LuisResult = new LuisResult {Query = "hi"},
                RecognizerResult = new RecognizerResult {Text = "hi"}
            };

            var serializerSettings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto};
            var serialized = JsonConvert.SerializeObject(luisTraceInfo, serializerSettings);
            var deserialized = JsonConvert.DeserializeObject<LuisTraceInfo>(serialized, serializerSettings);

            Assert.IsNotNull(deserialized);
            Assert.IsNotNull(deserialized.LuisModel);
            Assert.IsNotNull(deserialized.LuisOptions);
            Assert.IsNotNull(deserialized.LuisResult);
            Assert.IsNotNull(deserialized.RecognizerResult);
            Assert.AreEqual(luisTraceInfo.LuisModel.SubscriptionKey, deserialized.LuisModel.SubscriptionKey);
            Assert.AreEqual(luisTraceInfo.LuisOptions.Verbose, deserialized.LuisOptions.Verbose);
            Assert.AreEqual(luisTraceInfo.LuisResult.Query, deserialized.LuisResult.Query);
            Assert.AreEqual(luisTraceInfo.RecognizerResult.Text, deserialized.RecognizerResult.Text);
        }
    }
}
