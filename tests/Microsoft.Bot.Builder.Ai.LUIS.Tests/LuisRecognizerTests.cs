
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Cognitive.LUIS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Ai.LUIS.Tests
{
    [TestClass]
    /*
     * The LUIS application used in these unit tests is in TestData/TestLuistApp
     */
    public class LuisRecognizerTests
    {

        private readonly string _luisAppId = TestUtilities.GetKey("LUISAPPID");
        private readonly string _subscriptionKey = TestUtilities.GetKey("LUISAPPKEY");
        private readonly string _luisUriBase = TestUtilities.GetKey("LUISURIBASE");


        [TestMethod]
        public async Task SingleIntent_SimplyEntity()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true);
            var result = await luisRecognizer.Recognize("My name is Emad", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AlteredText);
            Assert.AreEqual("My name is Emad", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.AreEqual(1, result.Intents.Count);
            Assert.IsNotNull(result.Intents["SpecifyName"]);
            Assert.IsTrue((double)result.Intents["SpecifyName"] > 0 && (double)result.Intents["SpecifyName"] <= 1);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["Name"]);
            Assert.AreEqual("emad", (string)result.Entities["Name"].First);
            Assert.IsNotNull(result.Entities["$instance"]);
            Assert.IsNotNull(result.Entities["$instance"]["Name"]);
            Assert.AreEqual(11, (int)result.Entities["$instance"]["Name"].First["startIndex"]);
            Assert.AreEqual(14, (int)result.Entities["$instance"]["Name"].First["endIndex"]);
            AssertScore(result.Entities["$instance"]["Name"].First["score"]);
        }

        [TestMethod]
        public async Task MultipleIntents_PrebuiltEntity()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("Please deliver February 2nd 2001", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.AreEqual("Please deliver February 2nd 2001", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsTrue(result.Intents.Count > 1);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsTrue((double)result.Intents["Delivery"] > 0 && (double)result.Intents["Delivery"] <= 1);
            Assert.AreEqual("Delivery", result.GetTopScoringIntent().intent);
            Assert.IsTrue(result.GetTopScoringIntent().score > 0);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["builtin_number"]);
            Assert.AreEqual(2001, (int)result.Entities["builtin_number"].First);
            Assert.IsNotNull(result.Entities["builtin_ordinal"]);
            Assert.AreEqual(2, (int)result.Entities["builtin_ordinal"].First);
            Assert.IsNotNull(result.Entities["builtin_datetime"].First);
            Assert.AreEqual("2001-02-02", (string)result.Entities["builtin_datetime"].First["timex"].First);
            Assert.IsNotNull(result.Entities["$instance"]["builtin_number"]);
            Assert.AreEqual(28, (int)result.Entities["$instance"]["builtin_number"].First["startIndex"]);
            Assert.AreEqual(31, (int)result.Entities["$instance"]["builtin_number"].First["endIndex"]);
            Assert.AreEqual("2001", (string)result.Entities["$instance"]["builtin_number"].First["text"]);
            Assert.IsNotNull(result.Entities["$instance"]["builtin_datetime"]);
            Assert.AreEqual(15, (int)result.Entities["$instance"]["builtin_datetime"].First["startIndex"]);
            Assert.AreEqual(31, (int)result.Entities["$instance"]["builtin_datetime"].First["endIndex"]);
            Assert.AreEqual("february 2nd 2001", (string)result.Entities["$instance"]["builtin_datetime"].First["text"]);
        }

        [TestMethod]
        public async Task MultipleIntents_PrebuiltEntitiesWithMultiValues()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("Please deliver February 2nd 2001 in room 201", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual("Please deliver February 2nd 2001 in room 201", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["builtin_number"]);
            Assert.AreEqual(2, result.Entities["builtin_number"].Count());
            Assert.IsTrue(result.Entities["builtin_number"].Any(v => (int)v == 201));
            Assert.IsTrue(result.Entities["builtin_number"].Any(v => (int)v == 2001));
            Assert.IsNotNull(result.Entities["builtin_datetime"].First);
            Assert.AreEqual("2001-02-02", (string)result.Entities["builtin_datetime"].First["timex"].First);
        }

        [TestMethod]
        public async Task MultipleIntents_ListEntityWithSingleValue()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("I want to travel on united", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual("I want to travel on united", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsNotNull(result.Intents["Travel"]);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["Airline"]);
            Assert.AreEqual("United", result.Entities["Airline"][0][0]);
            Assert.IsNotNull(result.Entities["$instance"]);
            Assert.IsNotNull(result.Entities["$instance"]["Airline"]);
            Assert.AreEqual(20, result.Entities["$instance"]["Airline"][0]["startIndex"]);
            Assert.AreEqual(25, result.Entities["$instance"]["Airline"][0]["endIndex"]);
            Assert.AreEqual("united", result.Entities["$instance"]["Airline"][0]["text"]);
        }

        [TestMethod]
        public async Task MultipleIntents_ListEntityWithMultiValues()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("I want to travel on DL", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual("I want to travel on DL", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsNotNull(result.Intents["Travel"]);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["Airline"]);
            Assert.AreEqual(2, result.Entities["Airline"][0].Count());
            Assert.IsTrue(result.Entities["Airline"][0].Any(airline => (string)airline == "Delta"));
            Assert.IsTrue(result.Entities["Airline"][0].Any(airline => (string)airline == "Virgin"));
            Assert.IsNotNull(result.Entities["$instance"]);
            Assert.IsNotNull(result.Entities["$instance"]["Airline"]);
            Assert.AreEqual(20, result.Entities["$instance"]["Airline"][0]["startIndex"]);
            Assert.AreEqual(21, result.Entities["$instance"]["Airline"][0]["endIndex"]);
            Assert.AreEqual("dl", result.Entities["$instance"]["Airline"][0]["text"]);
        }

        [TestMethod]
        public async Task MultipleIntens_CompositeEntity()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("Please deliver it to 98033 WA", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual("Please deliver it to 98033 WA", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsNotNull(result.Entities);
            Assert.IsNull(result.Entities["builtin_number"]);
            Assert.IsNull(result.Entities["State"]);
            Assert.IsNotNull(result.Entities["Address"]);
            Assert.AreEqual(98033, result.Entities["Address"][0]["builtin_number"][0]);
            Assert.AreEqual("wa", result.Entities["Address"][0]["State"][0]);
            Assert.IsNotNull(result.Entities["$instance"]);
            Assert.IsNull(result.Entities["$instance"]["builtin_number"]);
            Assert.IsNull(result.Entities["$instance"]["State"]);
            Assert.IsNotNull(result.Entities["$instance"]["Address"]);
            Assert.AreEqual(21, result.Entities["$instance"]["Address"][0]["startIndex"]);
            Assert.AreEqual(28, result.Entities["$instance"]["Address"][0]["endIndex"]);
            AssertScore(result.Entities["$instance"]["Address"][0]["score"]);
            Assert.IsNotNull(result.Entities["Address"][0]["$instance"]);
            Assert.IsNotNull(result.Entities["Address"][0]["$instance"]["builtin_number"]);
            Assert.AreEqual(21, result.Entities["Address"][0]["$instance"]["builtin_number"][0]["startIndex"]);
            Assert.AreEqual(25, result.Entities["Address"][0]["$instance"]["builtin_number"][0]["endIndex"]);
            Assert.AreEqual("98033", result.Entities["Address"][0]["$instance"]["builtin_number"][0]["text"]);
            Assert.IsNotNull(result.Entities["Address"][0]["$instance"]["State"]);
            Assert.AreEqual(27, result.Entities["Address"][0]["$instance"]["State"][0]["startIndex"]);
            Assert.AreEqual(28, result.Entities["Address"][0]["$instance"]["State"][0]["endIndex"]);
            Assert.AreEqual("wa", result.Entities["Address"][0]["$instance"]["State"][0]["text"]);
            AssertScore(result.Entities["Address"][0]["$instance"]["State"][0]["score"]);
        }

        [TestMethod]
        public async Task MultipleDateTimeEntities()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("Book a table on Friday or tomorrow at 5 or tomorrow at 4", CancellationToken.None);
            Assert.IsNotNull(result.Entities["builtin_datetime"]);
            Assert.AreEqual(3, result.Entities["builtin_datetime"].Count());
            Assert.AreEqual(1, result.Entities["builtin_datetime"][0]["timex"].Count());
            Assert.AreEqual("XXXX-WXX-5", (string)result.Entities["builtin_datetime"][0]["timex"][0]);
            Assert.AreEqual(1, result.Entities["builtin_datetime"][0]["timex"].Count());
            Assert.AreEqual(2, result.Entities["builtin_datetime"][1]["timex"].Count());
            Assert.AreEqual(2, result.Entities["builtin_datetime"][2]["timex"].Count());
            Assert.IsTrue(((string)result.Entities["builtin_datetime"][1]["timex"][0]).EndsWith("T05"));
            Assert.IsTrue(((string)result.Entities["builtin_datetime"][1]["timex"][1]).EndsWith("T17"));
            Assert.IsTrue(((string)result.Entities["builtin_datetime"][2]["timex"][0]).EndsWith("T04"));
            Assert.IsTrue(((string)result.Entities["builtin_datetime"][2]["timex"][1]).EndsWith("T16"));
            Assert.AreEqual(3, result.Entities["$instance"]["builtin_datetime"].Count());
        }

        // Compare two JSON structures and ensure entity and intent scores are within delta
        private bool WithinDelta(JToken token1, JToken token2, double delta, bool compare = false)
        {
            bool withinDelta = true;
            if (token1.Type == JTokenType.Object && token2.Type == JTokenType.Object)
            {
                var obj1 = (JObject)token1;
                var obj2 = (JObject)token2;
                withinDelta = obj1.Count == obj2.Count;
                foreach (var property in obj1)
                {
                    if (!withinDelta)
                    {
                        break;
                    }
                    if (obj2.TryGetValue(property.Key, out JToken val2))
                    {
                        withinDelta = WithinDelta(property.Value, val2, delta, compare || property.Key == "score" || property.Key == "intents");
                    }
                }
            }
            else if (token1.Type == JTokenType.Array && token2.Type == JTokenType.Array)
            {
                var arr1 = (JArray)token1;
                var arr2 = (JArray)token2;
                withinDelta = arr1.Count() == arr2.Count();
                for (var i = 0; withinDelta && i < arr1.Count(); ++i)
                {
                    withinDelta = WithinDelta(arr1[i], arr2[i], delta);
                    if (!withinDelta)
                    {
                        break;
                    }
                }
            }
            else if (!token1.Equals(token2))
            {
                var val1 = (JValue)token1;
                var val2 = (JValue)token2;
                withinDelta = false;
                if (compare &&
                    double.TryParse((string)val1, out double num1)
                            && double.TryParse((string)val2, out double num2))
                {
                    withinDelta = Math.Abs(num1 - num2) < delta;
                }
            }
            return withinDelta;
        }

        private JObject JsonLuisResult(RecognizerResult result)
        {
            return new JObject(
                new JProperty("alteredText", result.AlteredText),
                new JProperty("entities", result.Entities),
                new JProperty("intents", result.Intents),
                new JProperty("text", result.Text));
        }

        // To create a file to test:
        // 1) Create a <name>.json file with an object { text:<query> } in it.
        // 2) Run this test which will fail and generate a <name>.json.new file.
        // 3) Check the .new file and if correct, replace the original .json file with it.
        public async Task TestJson(string file)
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var expectedPath = Path.Combine(@"..\..\..\TestData\", file);
            var newPath = expectedPath + ".new";
            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var expected = new StreamReader(expectedPath).ReadToEnd();
            dynamic expectedJson = JsonConvert.DeserializeObject(expected);
            var result = await luisRecognizer.Recognize((string)expectedJson.text, CancellationToken.None);
            var jsonResult = JsonLuisResult(result);
            if (!WithinDelta(expectedJson, jsonResult, 0.01))
            {
                using (var writer = new StreamWriter(newPath))
                {
                    writer.Write(jsonResult);
                }
                Assert.Fail($"Returned JSON in {newPath} != expected JSON in {expectedPath}");
            }
            else
            {
                File.Delete(expectedPath + ".new");
            }
        }

        [TestMethod]
        public async Task AllEntities()
        {
            await TestJson("Composite1.json");
            await TestJson("Composite2.json");
        }

        [TestMethod]
        public async Task TypedEntities()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }
            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var query = "fly from seattle to dallas";
            var untyped = await luisRecognizer.Recognize(query, CancellationToken.None);
            var typed = await luisRecognizer.Recognize<RecognizerResult>(query, CancellationToken.None);
            Assert.IsTrue(WithinDelta(JsonLuisResult(untyped), JsonLuisResult(typed), 0.0), "Weakly typed and strongly typed recognize does not match.");
        }

        private void AssertScore(JToken scoreToken)
        {
            var score = (double)scoreToken;
            Assert.IsTrue(score >= 0);
            Assert.IsTrue(score <= 1);
        }

        private bool EnvironmentVariablesDefined()
        {
            return _luisAppId != null && _subscriptionKey != null && _luisUriBase != null;
        }

        private IRecognizer GetLuisRecognizer(bool verbose = false, ILuisOptions luisOptions = null)
        {
            var luisRecognizerOptions = new LuisRecognizerOptions { Verbose = verbose };
            var luisModel = new LuisModel(_luisAppId, _subscriptionKey, new Uri(_luisUriBase), LuisApiVersion.V2);
            return new LuisRecognizer(luisModel, luisRecognizerOptions, luisOptions);
        }
    }
}
