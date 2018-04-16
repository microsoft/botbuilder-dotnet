using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Ai.LUIS.Tests
{
    [TestClass]
    public class RecognizerResultTests
    {
        [TestMethod]
        public void RecognizerResult_Serialization()
        {
            const string json = "{\"text\":\"hi      there\",\"alteredText\":\"hi there\",\"intents\":{\"Travel\":0.6},\"entities\":{\"Name\":\"Bob\"}}";
            var recognizerResult = new RecognizerResult
            {
                AlteredText = "hi there",
                Text = "hi      there",
                Entities = JObject.FromObject(new { Name= "Bob" }),
                Intents = JObject.FromObject(new { Travel = 0.6f })
            };

            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var serialized = JsonConvert.SerializeObject(recognizerResult, serializerSettings);
            
            Assert.AreEqual(json, serialized);
        }
    }
}
