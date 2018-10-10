// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.AI.Luis.Tests
{
    [TestClass]
    // The LUIS application used in these unit tests is in TestData/TestLuistApp.json
    public class LuisRecognizerTests
    {
        private readonly string _luisAppId = TestUtilities.GetKey("LUISAPPID", "ab48996d-abe2-4785-8eff-f18d15fc3560");
        private readonly string _subscriptionKey = TestUtilities.GetKey("LUISAPPKEY", "cc7bbcc0-3715-44f0-b7c9-d8fee333dce1");
        private readonly string _endpoint = TestUtilities.GetKey("LUISENDPOINT", "https://westus.api.cognitive.microsoft.com");

        private readonly RecognizerResult _mockedResults = new RecognizerResult
        {
            Intents = new Dictionary<string, IntentScore>()
                {
                    { "Test", new IntentScore { Score = 0.2 } },
                    { "Greeting", new IntentScore { Score = 0.4 } }
                }
        };
        // LUIS tests run off of recorded HTTP responses to avoid service dependencies.
        // To update the recorded responses:
        // 1) Change _mock to false below
        // 2) Set environment variable LUISAPPKEY = any valid LUIS endpoint key
        // 3) Run the LuisRecognizerTests
        // 4) If the http responses have changed there will be a file in this directory of<test>.json.new
        // 5) Run the review.cmd file to review each file if approved the new oracle file will replace the old one.
        // Changing this to false will cause running against the actual LUIS service.
        // This is useful in order to see if the oracles for mocking or testing have changed.
        private readonly bool _mock = true;

        [TestMethod]
        public async Task LuisRecognizer_Configuration()
        {
            var service = new LuisService
            {
                AppId = _luisAppId,
                SubscriptionKey = _subscriptionKey,
                Region = "westus"
            };

            const string utterance = "My name is Emad";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl()).WithPartialContent(utterance)
                .Respond("application/json", GetResponse("SingleIntent_SimplyEntity.json"));

            var luisRecognizer = new LuisRecognizer(service, null, false, new MockedHttpClientHandler(mockHttp.ToHttpClient()));

            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.IsNull(result.AlteredText);
            Assert.AreEqual(utterance, result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.AreEqual(1, result.Intents.Count);
            Assert.IsNotNull(result.Intents["SpecifyName"]);
            Assert.IsTrue(result.Intents["SpecifyName"].Score > 0 && result.Intents["SpecifyName"].Score <= 1);
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
        public async Task SingleIntent_SimplyEntity()
        {
            const string utterance = "My name is Emad";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl()).WithPartialContent(utterance)
                .Respond("application/json", GetResponse("SingleIntent_SimplyEntity.json"));

            var luisRecognizer = GetLuisRecognizer(mockHttp, verbose: true);
            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.IsNull(result.AlteredText);
            Assert.AreEqual(utterance, result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.AreEqual(1, result.Intents.Count);
            Assert.IsNotNull(result.Intents["SpecifyName"]);
            Assert.IsTrue(result.Intents["SpecifyName"].Score > 0 && result.Intents["SpecifyName"].Score <= 1);
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
            const string utterance = "Please deliver February 2nd 2001";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl()).WithPartialContent(utterance)
                .Respond("application/json", GetResponse("MultipleIntents_PrebuiltEntity.json"));

            var luisRecognizer = GetLuisRecognizer(mockHttp, true, new LuisPredictionOptions { IncludeAllIntents = true });
            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(utterance, result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsTrue(result.Intents.Count > 1);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsTrue(result.Intents["Delivery"].Score > 0 && result.Intents["Delivery"].Score <= 1);
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
            const string utterance = "Please deliver February 2nd 2001 in room 201";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl()).WithPartialContent(utterance)
                .Respond("application/json", GetResponse("MultipleIntents_PrebuiltEntitiesWithMultiValues.json"));

            var luisRecognizer = GetLuisRecognizer(mockHttp, true, new LuisPredictionOptions { IncludeAllIntents = true });
            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual(utterance, result.Text);
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
            const string utterance = "I want to travel on united";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl()).WithPartialContent(utterance)
                .Respond("application/json", GetResponse("MultipleIntents_ListEntityWithSingleValue.json"));

            var luisRecognizer = GetLuisRecognizer(mockHttp, true, new LuisPredictionOptions { IncludeAllIntents = true });
            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual(utterance, result.Text);
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
            const string utterance = "I want to travel on DL";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl()).WithPartialContent(utterance)
                .Respond("application/json", GetResponse("MultipleIntents_ListEntityWithMultiValues.json"));

            var luisRecognizer = GetLuisRecognizer(mockHttp, true, new LuisPredictionOptions { IncludeAllIntents = true });
            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual(utterance, result.Text);
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
        public async Task MultipleIntents_CompositeEntityModel()
        {
            const string utterance = "Please deliver it to 98033 WA";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl()).WithPartialContent(utterance)
                .Respond("application/json", GetResponse("MultipleIntents_CompositeEntityModel.json"));

            var luisRecognizer = GetLuisRecognizer(mockHttp, true, new LuisPredictionOptions { IncludeAllIntents = true });
            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual(utterance, result.Text);
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
            const string utterance = "Book a table on Friday or tomorrow at 5 or tomorrow at 4";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl()).WithPartialContent(utterance)
                .Respond("application/json", GetResponse("MultipleDateTimeEntities.json"));

            var luisRecognizer = GetLuisRecognizer(mockHttp, true, new LuisPredictionOptions { IncludeAllIntents = true });
            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

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
        public async Task V1DatetimeResolution()
        {
            const string utterance = "at 4";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl()).WithPartialContent(utterance)
                .Respond("application/json", GetResponse("V1DatetimeResolution.json"));

            var luisRecognizer = GetLuisRecognizer(mockHttp, true, new LuisPredictionOptions { IncludeAllIntents = true });
            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

            Assert.IsNotNull(result.Entities["datetime_time"]);
            Assert.AreEqual(1, result.Entities["datetime_time"].Count());
            Assert.AreEqual("ampm", (string)result.Entities["datetime_time"][0]["comment"]);
            Assert.AreEqual("T04", (string)result.Entities["datetime_time"][0]["time"]);
            Assert.AreEqual(1, result.Entities["$instance"]["datetime_time"].Count());
        }

        // To create a file to test:
        // 1) Create a <name>.json file with an object { Text:<query> } in it.
        // 2) Run this test which will fail and generate a <name>.json.new file.
        // 3) Check the .new file and if correct, replace the original .json file with it.
        public async Task TestJson<T>(string file) where T : IRecognizerConvert, new()
        {
            var expectedPath = GetFilePath(file);
            var newPath = expectedPath + ".new";

            using (var expectedJsonReader = new JsonTextReader(new StreamReader(expectedPath)))
            {
                var expectedJson = await JToken.ReadFromAsync(expectedJsonReader);
                using (var mockResponse = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(expectedJson["luisResult"]))))
                {
                    var text = expectedJson["text"] ?? expectedJson["Text"];
                    var query = text.ToString();
                    var context = GetContext(query);

                    var mockHttp = new MockHttpMessageHandler();
                    mockHttp.When(GetRequestUrl()).WithPartialContent(query)
                        .Respond("application/json", mockResponse);

                    var luisRecognizer = GetLuisRecognizer(mockHttp, true, new LuisPredictionOptions { IncludeAllIntents = true });
                    var typedResult = await luisRecognizer.RecognizeAsync<T>(context, CancellationToken.None);
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
        }

        [TestMethod]
        public async Task TraceActivity()
        {
            const string utterance = @"My name is Emad";
            const string botResponse = @"Hi Emad";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(GetRequestUrl()).WithPartialContent(utterance)
                .Respond("application/json", GetResponse("TraceActivity.json"));

            var adapter = new TestAdapter(null, true);
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                if (context.Activity.Text == utterance)
                {
                    var luisRecognizer = GetLuisRecognizer(mockHttp, verbose: true);
                    await luisRecognizer.RecognizeAsync(context, CancellationToken.None).ConfigureAwait(false);
                    await context.SendActivityAsync(botResponse);
                }
            })
                .Test(utterance, activity =>
                {
                    var traceActivity = activity as ITraceActivity;
                    Assert.IsNotNull(traceActivity);
                    Assert.AreEqual(LuisRecognizer.LuisTraceType, traceActivity.ValueType);
                    Assert.AreEqual(LuisRecognizer.LuisTraceLabel, traceActivity.Label);

                    var luisTraceInfo = JObject.FromObject(traceActivity.Value);
                    Assert.IsNotNull(luisTraceInfo);
                    Assert.IsNotNull(luisTraceInfo["recognizerResult"]);
                    Assert.IsNotNull(luisTraceInfo["luisResult"]);
                    Assert.IsNotNull(luisTraceInfo["luisOptions"]);
                    Assert.IsNotNull(luisTraceInfo["luisModel"]);

                    var recognizerResult = luisTraceInfo["recognizerResult"].ToObject<RecognizerResult>();
                    Assert.AreEqual(recognizerResult.Text, utterance);
                    Assert.IsNotNull(recognizerResult.Intents["SpecifyName"]);
                    Assert.AreEqual(luisTraceInfo["luisResult"]["query"], utterance);
                    Assert.AreEqual(luisTraceInfo["luisModel"]["ModelID"], _luisAppId);
                    Assert.AreEqual(luisTraceInfo["luisOptions"]["Staging"], default(bool?));

                }, "luisTraceInfo")
                .Send(utterance)
                .AssertReply(botResponse, "passthrough")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Composite1() => await TestJson<RecognizerResult>("Composite1.json");

        [TestMethod]
        public async Task Composite2() => await TestJson<RecognizerResult>("Composite2.json");

        [TestMethod]
        public async Task PrebuiltDomains() => await TestJson<RecognizerResult>("Prebuilt.json");

        [TestMethod]
        public async Task Patterns() => await TestJson<RecognizerResult>("Patterns.json");

        [TestMethod]
        public async Task TypedEntities() => await TestJson<Contoso_App>("Typed.json");

        [TestMethod]
        public async Task TypedPrebuiltDomains() => await TestJson<Contoso_App>("TypedPrebuilt.json");

        [TestMethod]
        public void TopIntentReturnsTopIntent()
        {
            var greetingIntent = LuisRecognizer.TopIntent(_mockedResults);
            Assert.AreEqual(greetingIntent, "Greeting");
        }

        [TestMethod]
        public void TopIntentReturnsDefaultIntentIfMinScoreIsHigher()
        {
            var defaultIntent = LuisRecognizer.TopIntent(_mockedResults, minScore: 0.5);
            Assert.AreEqual(defaultIntent, "None");
        }

        [TestMethod]
        public void TopIntentReturnsDefaultIntentIfProvided()
        {
            var defaultIntent = LuisRecognizer.TopIntent(_mockedResults, "Test2", 0.5);
            Assert.AreEqual(defaultIntent, "Test2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TopIntentThrowsArgumentNullExceptionIfResultsIsNull()
        {
            RecognizerResult nullResults = null;
            var noIntent = LuisRecognizer.TopIntent(nullResults);
        }

        [TestMethod]
        public void TopIntentReturnsTopIntentIfScoreEqualsMinScore()
        {
            var defaultIntent = LuisRecognizer.TopIntent(_mockedResults, minScore: 0.4);
            Assert.AreEqual(defaultIntent, "Greeting");
        }

        // Compare two JSON structures and ensure entity and intent scores are within delta
        private bool WithinDelta(JToken token1, JToken token2, double delta, bool compare = false)
        {
            var withinDelta = true;
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

                    withinDelta = obj2.TryGetValue(property.Key, out var val2) && WithinDelta(property.Value, val2, delta, compare || property.Key == "score" || property.Key == "intents");
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
                        double.TryParse((string)val1, out var num1)
                                && double.TryParse((string)val2, out var num2))
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

        private JObject Json<T>(T result)
            => (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result, new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }));

        private void AssertScore(JToken scoreToken)
        {
            var score = (double)scoreToken;
            Assert.IsTrue(score >= 0);
            Assert.IsTrue(score <= 1);
        }

        private static TurnContext GetContext(string utterance)
        {
            var b = new TestAdapter();
            var a = new Activity
            {
                Type = ActivityTypes.Message,
                Text = utterance,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount()
            };
            return new TurnContext(b, a);
        }

        private IRecognizer GetLuisRecognizer(MockHttpMessageHandler messageHandler, bool verbose = false, LuisPredictionOptions options = null)
        {
            var luisApp = new LuisApplication(_luisAppId, _subscriptionKey, _endpoint);
            return new LuisRecognizer(luisApp, options, verbose, _mock ? new MockedHttpClientHandler(messageHandler.ToHttpClient()) : null);
        }

        private string GetRequestUrl() => $"{_endpoint}/luis/v2.0/apps/{_luisAppId}";

        // Access the checked-in oracles so that if they are changed you can compare the changes and easily modify them.
        private const string _testData = @"..\..\..\TestData\";

        private Stream GetResponse(string fileName)
        {
            var path = Path.Combine(_testData, fileName);
            return File.OpenRead(path);
        }

        private string GetFilePath(string fileName)
        {
            var path = Path.Combine(_testData, fileName);
            return path;
        }
    }

    public class MockedHttpClientHandler : HttpClientHandler
    {
        private readonly HttpClient client;

        public MockedHttpClientHandler(HttpClient client)
        {
            this.client = client;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var mockedRequest = new HttpRequestMessage()
            {
                RequestUri = request.RequestUri,
                Content = request.Content,
                Method = request.Method
            };
            return client.SendAsync(mockedRequest, cancellationToken);
        }
    }
}
