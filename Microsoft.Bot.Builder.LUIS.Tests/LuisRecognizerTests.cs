using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.LUIS.Tests
{
    [TestClass]
    /*
     * The LUIS application used in these unit tests is in TestData/TestLuistApp
     */
    public class LuisRecognizerTests
    {
        [TestMethod]
        public async Task SingleIntent_SimplyEntity()
        {
            var luisRecognizer = GetLuisRecognizer();
            var result = await luisRecognizer.Recognize("My name is Emad", CancellationToken.None, true);
            Assert.IsNotNull(result);
            Assert.AreEqual("My name is Emad", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.AreEqual(1, result.Intents.Count);
            Assert.IsNotNull(result.Intents["SpecifyName"]);
            Assert.IsTrue((double)result.Intents["SpecifyName"] > 0 &&(double) result.Intents["SpecifyName"] <= 1);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["Name"]);
            Assert.AreEqual("emad", (string)result.Entities["Name"].First);
            Assert.IsNotNull(result.Entities["$instance"]);
            Assert.IsNotNull(result.Entities["$instance"]["Name"]);
            Assert.AreEqual(11, (int)result.Entities["$instance"]["Name"].First["startIndex"]);
            Assert.AreEqual(14, (int)result.Entities["$instance"]["Name"].First["endIndex"]);
            Assert.IsTrue((double)result.Entities["$instance"]["Name"].First["score"] > 0 && (double)result.Entities["$instance"]["Name"].First["score"] <= 1);
        }

        [TestMethod]
        public async Task MultipleIntents_PrebuiltEntity()
        {
            var luisRecognizer = GetLuisRecognizer(new LuisRequest(string.Empty){Verbose = true});
            var result = await luisRecognizer.Recognize("Please deliver February 2nd 2001", CancellationToken.None, true);
            Assert.IsNotNull(result);
            Assert.AreEqual("Please deliver February 2nd 2001", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsTrue(result.Intents.Count > 1);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsTrue((double)result.Intents["Delivery"] > 0 && (double)result.Intents["Delivery"] <= 1);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["builtin_number"]);
            Assert.AreEqual(2001, (int)result.Entities["builtin_number"].First);
            Assert.IsNotNull(result.Entities["builtin_datetimeV2_date"]);
            Assert.AreEqual("2001-02-02", (string)result.Entities["builtin_datetimeV2_date"].First);
            Assert.IsNotNull(result.Entities["$instance"]["builtin_number"]);
            Assert.AreEqual(28, (int)result.Entities["$instance"]["builtin_number"].First["startIndex"]);
            Assert.AreEqual(31, (int)result.Entities["$instance"]["builtin_number"].First["endIndex"]);
            Assert.AreEqual("2001", (string)result.Entities["$instance"]["builtin_number"].First["text"]);
            Assert.IsNotNull(result.Entities["$instance"]["builtin_datetimeV2_date"]);
            Assert.AreEqual(15, (int)result.Entities["$instance"]["builtin_datetimeV2_date"].First["startIndex"]);
            Assert.AreEqual(31, (int)result.Entities["$instance"]["builtin_datetimeV2_date"].First["endIndex"]);
            Assert.AreEqual("february 2nd 2001", (string)result.Entities["$instance"]["builtin_datetimeV2_date"].First["text"]);
        }

        [TestMethod]
        public async Task MultipleIntents_PrebuiltEntitiesWithMultiValues()
        {
            var luisRecognizer = GetLuisRecognizer(new LuisRequest(string.Empty) { Verbose = true });
            var result = await luisRecognizer.Recognize("Please deliver February 2nd 2001 in room 201", CancellationToken.None, true);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual("Please deliver February 2nd 2001 in room 201", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["builtin_number"]);
            Assert.AreEqual(2, result.Entities["builtin_number"].Count());
            Assert.IsTrue(result.Entities["builtin_number"].Any(v =>(int)v == 201));
            Assert.IsTrue(result.Entities["builtin_number"].Any(v => (int)v == 2001));
            Assert.IsNotNull(result.Entities["builtin_datetimeV2_date"]);
            Assert.AreEqual("2001-02-02", (string)result.Entities["builtin_datetimeV2_date"].First);
        }

        private static IRecognizer GetLuisRecognizer(ILuisOptions luisOptions = null)
        {
            var luisModel = new LuisModel("6209a76f-e836-413b-ba92-a5772d1b2087", "f2eef6a1cab345b9b4b53743357e869f", new Uri("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/"), LuisApiVersion.V2);
            return new LuisRecognizer(luisModel, luisOptions);
        }
    }
}
