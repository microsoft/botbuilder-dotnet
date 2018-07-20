// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.Ai.LUIS.Tests
{
    [TestClass]
    //
    // The LUIS application used in these unit tests is in TestData/TestLuistApp
    //
    public class LuisRecognizerTests
    {
        private const string _luisAppId = "dummy-app-id";
        private const string _subscriptionKey = "dummy-subscription-key";
        private const string _luisUriBase = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/";

        [TestMethod]
        public async Task SingleIntent_SimplyEntity()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl($"subscription-key={_subscriptionKey}&q=My name is Emad&log=True"))
                .Respond("application/json", GetResponse("SingleIntent_SimplyEntity.json"));

            var luisRecognizer = GetLuisRecognizer(mockHttp, true);
            var result = await luisRecognizer.RecognizeAsync("My name is Emad", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNull(result.AlteredText);
            Assert.AreEqual("My name is Emad", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.AreEqual(1, result.Intents.Count);
            Assert.IsNotNull(result.Intents["SpecifyName"]);
            Assert.IsTrue((double)result.Intents["SpecifyName"]["score"] > 0 && (double)result.Intents["SpecifyName"]["score"] <= 1);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["Name"]);
            Assert.AreEqual("emad", (string)result.Entities["Name"].First);
            Assert.IsNotNull(result.Entities["$instance"]);
            Assert.IsNotNull(result.Entities["$instance"]["Name"]);
            Assert.AreEqual(11, (int)result.Entities["$instance"]["Name"].First["startIndex"]);
            Assert.AreEqual(15, (int)result.Entities["$instance"]["Name"].First["endIndex"]);
            AssertScore(result.Entities["$instance"]["Name"].First["score"]);
        }

        [TestMethod]
        public async Task MultipleIntents_PrebuiltEntity()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl($"subscription-key={_subscriptionKey}&q=Please deliver February 2nd 2001&log=True&verbose=True"))
                .Respond("application/json", GetResponse("MultipleIntents_PrebuiltEntity.json"));

            var luisRecognizer = GetLuisRecognizer(mockHttp, verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.RecognizeAsync("Please deliver February 2nd 2001", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.AreEqual("Please deliver February 2nd 2001", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsTrue(result.Intents.Count > 1);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsTrue((double)result.Intents["Delivery"]["score"] > 0 && (double)result.Intents["Delivery"]["score"] <= 1);
            Assert.AreEqual("Delivery", result.GetTopScoringIntent().intent);
            Assert.IsTrue(result.GetTopScoringIntent().score > 0);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["number"]);
            Assert.AreEqual(2001, (int)result.Entities["number"].First);
            Assert.IsNotNull(result.Entities["ordinal"]);
            Assert.AreEqual(2, (int)result.Entities["ordinal"].First);
            Assert.IsNotNull(result.Entities["datetime"].First);
            Assert.AreEqual("2001-02-02", (string)result.Entities["datetime"].First["timex"].First);
            Assert.IsNotNull(result.Entities["$instance"]["number"]);
            Assert.AreEqual(28, (int)result.Entities["$instance"]["number"].First["startIndex"]);
            Assert.AreEqual(32, (int)result.Entities["$instance"]["number"].First["endIndex"]);
            Assert.AreEqual("2001", result.Text.Substring(28, 32 - 28));
            Assert.IsNotNull(result.Entities["$instance"]["datetime"]);
            Assert.AreEqual(15, (int)result.Entities["$instance"]["datetime"].First["startIndex"]);
            Assert.AreEqual(32, (int)result.Entities["$instance"]["datetime"].First["endIndex"]);
            Assert.AreEqual("february 2nd 2001", (string)result.Entities["$instance"]["datetime"].First["text"]);
        }

        [TestMethod]
        public async Task MultipleIntents_PrebuiltEntitiesWithMultiValues()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl($"subscription-key={_subscriptionKey}&q=Please deliver February 2nd 2001 in room 201&log=True&verbose=True"))
                .Respond("application/json", GetResponse("MultipleIntents_PrebuiltEntitiesWithMultiValues.json"));

            var luisRecognizer = GetLuisRecognizer(mockHttp, verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.RecognizeAsync("Please deliver February 2nd 2001 in room 201", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual("Please deliver February 2nd 2001 in room 201", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["number"]);
            Assert.AreEqual(2, result.Entities["number"].Count());
            Assert.IsTrue(result.Entities["number"].Any(v => (int)v == 201));
            Assert.IsTrue(result.Entities["number"].Any(v => (int)v == 2001));
            Assert.IsNotNull(result.Entities["datetime"].First);
            Assert.AreEqual("2001-02-02", (string)result.Entities["datetime"].First["timex"].First);
        }

        [TestMethod]
        public async Task MultipleIntents_ListEntityWithSingleValue()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl($"subscription-key={_subscriptionKey}&q=I want to travel on united&log=True&verbose=True"))
                .Respond("application/json", GetResponse("MultipleIntents_ListEntityWithSingleValue.json"));
            
            var luisRecognizer = GetLuisRecognizer(mockHttp, verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.RecognizeAsync("I want to travel on united", CancellationToken.None);
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
            Assert.AreEqual(26, result.Entities["$instance"]["Airline"][0]["endIndex"]);
            Assert.AreEqual("united", result.Entities["$instance"]["Airline"][0]["text"]);
        }

        [TestMethod]
        public async Task MultipleIntents_ListEntityWithMultiValues()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl($"subscription-key={_subscriptionKey}&q=I want to travel on DL&log=True&verbose=True"))
                .Respond("application/json", GetResponse("MultipleIntents_ListEntityWithMultiValues.json"));
            
            var luisRecognizer = GetLuisRecognizer(mockHttp, verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.RecognizeAsync("I want to travel on DL", CancellationToken.None);
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
            Assert.AreEqual(22, result.Entities["$instance"]["Airline"][0]["endIndex"]);
            Assert.AreEqual("dl", result.Entities["$instance"]["Airline"][0]["text"]);
        }

        [TestMethod]
        public async Task MultipleIntens_CompositeEntity()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl($"subscription-key={_subscriptionKey}&q=Please deliver it to 98033 WA&log=True&verbose=True"))
                .Respond("application/json", GetResponse("MultipleIntens_CompositeEntity.json"));
            
            var luisRecognizer = GetLuisRecognizer(mockHttp, verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.RecognizeAsync("Please deliver it to 98033 WA", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual("Please deliver it to 98033 WA", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsNotNull(result.Entities);
            Assert.IsNull(result.Entities["number"]);
            Assert.IsNull(result.Entities["State"]);
            Assert.IsNotNull(result.Entities["Address"]);
            Assert.AreEqual(98033, result.Entities["Address"][0]["number"][0]);
            Assert.AreEqual("wa", result.Entities["Address"][0]["State"][0]);
            Assert.IsNotNull(result.Entities["$instance"]);
            Assert.IsNull(result.Entities["$instance"]["number"]);
            Assert.IsNull(result.Entities["$instance"]["State"]);
            Assert.IsNotNull(result.Entities["$instance"]["Address"]);
            Assert.AreEqual(21, result.Entities["$instance"]["Address"][0]["startIndex"]);
            Assert.AreEqual(29, result.Entities["$instance"]["Address"][0]["endIndex"]);
            AssertScore(result.Entities["$instance"]["Address"][0]["score"]);
            Assert.IsNotNull(result.Entities["Address"][0]["$instance"]);
            Assert.IsNotNull(result.Entities["Address"][0]["$instance"]["number"]);
            Assert.AreEqual(21, result.Entities["Address"][0]["$instance"]["number"][0]["startIndex"]);
            Assert.AreEqual(26, result.Entities["Address"][0]["$instance"]["number"][0]["endIndex"]);
            Assert.AreEqual("98033", result.Entities["Address"][0]["$instance"]["number"][0]["text"]);
            Assert.IsNotNull(result.Entities["Address"][0]["$instance"]["State"]);
            Assert.AreEqual(27, result.Entities["Address"][0]["$instance"]["State"][0]["startIndex"]);
            Assert.AreEqual(29, result.Entities["Address"][0]["$instance"]["State"][0]["endIndex"]);
            Assert.AreEqual("wa", result.Entities["Address"][0]["$instance"]["State"][0]["text"]);
            Assert.AreEqual("WA", result.Text.Substring(27, 29 - 27));
            AssertScore(result.Entities["Address"][0]["$instance"]["State"][0]["score"]);
        }

        [TestMethod]
        public async Task MultipleDateTimeEntities()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl($"subscription-key={_subscriptionKey}&q=Book a table on Friday or tomorrow at 5 or tomorrow at 4&log=True&verbose=True"))
                .Respond("application/json", GetResponse("MultipleDateTimeEntities.json"));
            
            var luisRecognizer = GetLuisRecognizer(mockHttp, verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.RecognizeAsync("Book a table on Friday or tomorrow at 5 or tomorrow at 4", CancellationToken.None);
            Assert.IsNotNull(result.Entities["datetime"]);
            Assert.AreEqual(3, result.Entities["datetime"].Count());
            Assert.AreEqual(1, result.Entities["datetime"][0]["timex"].Count());
            Assert.AreEqual("XXXX-WXX-5", (string)result.Entities["datetime"][0]["timex"][0]);
            Assert.AreEqual(1, result.Entities["datetime"][0]["timex"].Count());
            Assert.AreEqual(2, result.Entities["datetime"][1]["timex"].Count());
            Assert.AreEqual(2, result.Entities["datetime"][2]["timex"].Count());
            Assert.IsTrue(((string)result.Entities["datetime"][1]["timex"][0]).EndsWith("T05"));
            Assert.IsTrue(((string)result.Entities["datetime"][1]["timex"][1]).EndsWith("T17"));
            Assert.IsTrue(((string)result.Entities["datetime"][2]["timex"][0]).EndsWith("T04"));
            Assert.IsTrue(((string)result.Entities["datetime"][2]["timex"][1]).EndsWith("T16"));
            Assert.AreEqual(3, result.Entities["$instance"]["datetime"].Count());
        }

        [TestMethod]
        public async Task Composite1()
        {
            await TestJson<RecognizerResult>("Composite1.json");
        }

        [TestMethod]
        public async Task Composite2()
        {
            await TestJson<RecognizerResult>("Composite2.json");
        }

        [TestMethod]
        public async Task PrebuiltDomains()
        {
            await TestJson<RecognizerResult>("Prebuilt.json");
        }

        [TestMethod]
        public async Task Patterns()
        {
            await TestJson<RecognizerResult>("Patterns.json");
        }

        [TestMethod]
        public async Task TypedEntities()
        {
            await TestJson<Contoso_App>("Typed.json");
        }

        [TestMethod]
        public async Task TypedPrebuiltDomains()
        {
            await TestJson<Contoso_App>("TypedPrebuilt.json");
        }

        [TestMethod]
        public async Task UnavailableService()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond(HttpStatusCode.BadRequest);

            var luisRecognizer = GetLuisRecognizer(mockHttp);
            var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() => luisRecognizer.RecognizeAsync("test", CancellationToken.None));
            Assert.AreEqual("Response status code does not indicate success: 400 (Bad Request).", ex.Message);
        }

        [TestMethod]
        public async Task ErrorService()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond(HttpStatusCode.InternalServerError);

            var luisRecognizer = GetLuisRecognizer(mockHttp);
            var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() => luisRecognizer.RecognizeAsync("test", CancellationToken.None));
            Assert.AreEqual("Response status code does not indicate success: 500 (Internal Server Error).", ex.Message);
        }

        [TestMethod]
        public async Task JsonErrorService()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("*").Respond("application/json", "error message");

            var luisRecognizer = GetLuisRecognizer(mockHttp);
            var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() => luisRecognizer.RecognizeAsync("test", CancellationToken.None));
            Assert.AreEqual("Unable to deserialize the LUIS response.", ex.Message);
        }

        // To create a file to test:
        // 1) Create a <name>.json file with an object { Text:<query> } in it.
        // 2) Run this test which will fail and generate a <name>.json.new file.
        // 3) Check the .new file and if correct, replace the original .json file with it.
        public async Task TestJson<T>(string file) where T : IRecognizerConvert, new()
        {
            var expectedPath = GetFilePath(file);
            var mockPath = GetFilePath("Mock_" + file);
            var newPath = expectedPath + ".new";

            using (var expectedJsonReader = new JsonTextReader(new StreamReader(expectedPath)))
            {
                var expectedJson = await JToken.ReadFromAsync(expectedJsonReader);
                var text = expectedJson["text"] ?? expectedJson["Text"];
                var query = text.ToString();

                var mockHttp = new MockHttpMessageHandler();
                mockHttp.When(GetRequestUrl($"subscription-key={_subscriptionKey}&q={Uri.EscapeDataString(query)}&log=True&verbose=True"))
                    .Respond("application/json", GetResponse(mockPath));

                var luisRecognizer = GetLuisRecognizer(mockHttp, verbose: true, luisOptions: new LuisRequest { Verbose = true });
                var typedResult = await luisRecognizer.RecognizeAsync<T>(query, CancellationToken.None);
                var typedJson = Json(typedResult);
                if (!WithinDelta(expectedJson, typedJson, 0.1))
                {
                    using (var writer = new StreamWriter(newPath))
                    {
                        writer.Write(typedJson);
                    }
                    Assert.Fail($"Returned JSON in {newPath} != expected JSON in {expectedPath}");
                }
                else
                {
                    File.Delete(expectedPath + ".new");
                }
            }
        }

        private JObject Json<T>(T result)
        {
            return (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result, new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }));
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
                    withinDelta = obj2.TryGetValue(property.Key, out JToken val2) && WithinDelta(property.Value, val2, delta, compare || property.Key == "score" || property.Key == "intents");
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
                if (token1.Type == token2.Type)
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
                else
                {
                    withinDelta = false;
                }
            }
            return withinDelta;
        }

        private void AssertScore(JToken scoreToken)
        {
            var score = (double)scoreToken;
            Assert.IsTrue(score >= 0);
            Assert.IsTrue(score <= 1);
        }
        
        private IRecognizer GetLuisRecognizer(HttpMessageHandler messageHandler, bool verbose = false, ILuisOptions luisOptions = null)
        {
            var client = new HttpClient(messageHandler);
            var luisRecognizerOptions = new LuisRecognizerOptions { Verbose = verbose };
            var luisModel = new LuisModel(_luisAppId, _subscriptionKey, new Uri(_luisUriBase), LuisApiVersion.V2);

            return new LuisRecognizer(luisModel, luisRecognizerOptions, luisOptions, client);
        }
        
        private string GetRequestUrl(string query)
        {
            return $"{_luisUriBase}{_luisAppId}?{query}";
        }

        private Stream GetResponse(string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
            return File.OpenRead(path);
        }

        private string GetFilePath(string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
            return path;
        }
    }
}
