using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis.TestUtils
{
    public class Utils
    {
        public static TurnContext GetContext(string utterance)
        {
            var testAdapter = new TestAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = utterance,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(),
            };
            return new TurnContext(testAdapter, activity);
        }

        public static TurnContext GetNonMessageContext(string utterance)
        {
            var b = new TestAdapter();
            var a = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                Text = utterance,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(),
            };
            return new TurnContext(b, a);
        }

        public static void AssertScore(JToken scoreToken)
        {
            var score = (double)scoreToken;
            Assert.IsTrue(score >= 0);
            Assert.IsTrue(score <= 1);
        }

        // Compare two JSON structures and ensure entity and intent scores are within delta
        public static bool WithinDelta(JToken expected, JToken actual, double delta, bool compare = false)
        {
            var withinDelta = true;
            if (expected.Type == JTokenType.Object && actual.Type == JTokenType.Object)
            {
                var obj1 = (JObject)expected;
                var obj2 = (JObject)actual;
                withinDelta = obj1.Count == obj2.Count;
                if (!withinDelta)
                {
                    // Try removing score which is added by V3 to some prebuilts
                    if (obj1.Remove("score"))
                    {
                        withinDelta = obj1.Count == obj2.Count;
                    }
                }

                foreach (var property in obj1)
                {
                    if (!withinDelta)
                    {
                        break;
                    }

                    withinDelta = obj2.TryGetValue(property.Key, out var val2) && WithinDelta(property.Value, val2, delta, compare || property.Key == "score" || property.Key == "intents");
                }
            }
            else if (expected.Type == JTokenType.Array && actual.Type == JTokenType.Array)
            {
                var arr1 = (JArray)expected;
                var arr2 = (JArray)actual;
                withinDelta = arr1.Count == arr2.Count;
                for (var i = 0; withinDelta && i < arr1.Count; ++i)
                {
                    withinDelta = WithinDelta(arr1[i], arr2[i], delta);
                    if (!withinDelta)
                    {
                        break;
                    }
                }
            }
            else if (!expected.Equals(actual))
            {
                if (expected.Type == actual.Type)
                {
                    if (expected.Type == JTokenType.String)
                    {
                        withinDelta = expected.Value<string>().Equals(actual.Value<string>(), StringComparison.InvariantCultureIgnoreCase);
                    }
                    else
                    {
                        var val1 = (JValue)expected;
                        var val2 = (JValue)actual;
                        withinDelta = false;
                        if (compare &&
                            double.TryParse((string)val1, out var num1)
                                    && double.TryParse((string)val2, out var num2))
                        {
                            withinDelta = Math.Abs(num1 - num2) < delta;
                        }
                    }
                }
                else
                {
                    withinDelta = false;
                }
            }

            return withinDelta;
        }

        public static JToken SortJSON(JToken source)
        {
            var result = source;
            if (source is JObject obj)
            {
                var nobj = new JObject();
                foreach (var property in obj.Properties().OrderBy(p => p.Name))
                {
                    nobj.Add(property.Name, SortJSON(property.Value));
                }

                result = nobj;
            }
            else if (source is JArray arr)
            {
                var narr = new JArray();
                foreach (var elt in arr)
                {
                    narr.Add(SortJSON(elt));
                }

                result = narr;
            }

            return result;
        }

        public static JObject Json<T>(T result, string version, JObject oracle)
        {
            var json = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result, new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }));
            if (oracle["v2"] != null)
            {
                json["v2"] = oracle["v2"];
            }

            if (oracle["v3"] != null)
            {
                json["v3"] = oracle["v3"];
            }

            json[version] = json["luisResult"];
            json.Remove("luisResult");
            return (JObject)Utils.SortJSON(json);
        }

        // To create a file to test:
        // 1) Create a <name>.json file with an object { text:<query> } in it.
        // 2) Run this test which will fail and generate a <name>.json.new file.
        // 3) Check the .new file and if correct, replace the original .json file with it.
        // The version parameter controls where in the expected json the luisResult is put.  This allows multiple endpoint responses like from
        // LUIS V2 and V3 endpoints.
        public static async Task TestJsonOracle<T>(string expectedPath, string version, Func<JObject, IRecognizer> buildRecognizer)
            where T : IRecognizerConvert, new()
        {
            JObject expectedJson;
            using (var expectedJsonReader = new JsonTextReader(new StreamReader(expectedPath)))
            {
                expectedJson = (JObject)await JToken.ReadFromAsync(expectedJsonReader);
            }

            var newPath = expectedPath + ".new";
            var query = expectedJson["text"].ToString();
            var context = GetContext(query);
            var luisRecognizer = buildRecognizer(expectedJson);
            var typedResult = await luisRecognizer.RecognizeAsync<T>(context, CancellationToken.None);
            var typedJson = Utils.Json(typedResult, version, expectedJson);

            // Threshold is 0.0 so when hitting endpoint get exact and when mocking isn't needed.
            if (!Utils.WithinDelta(expectedJson, typedJson, 0.0))
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
