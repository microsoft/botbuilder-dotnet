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
        public static bool WithinDelta(JToken token1, JToken token2, double delta, bool compare = false)
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

            if (!Utils.WithinDelta(expectedJson, typedJson, 0.1))
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
