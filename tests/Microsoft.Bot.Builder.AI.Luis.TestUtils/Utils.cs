// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis.TestUtils
{
    public class Utils
    {
        // These are properties that are found only in V3 or V2.
        // We copy them over to allow for common oracle files.
        // subtype is V2 only, the others are from V3
        private static readonly List<string> _mismatches = new List<string> { "score", "modelType", "recognitionSources", "subtype" };

        public static void AssertScore(JToken scoreToken)
        {
            var score = (double)scoreToken;
            Assert.IsTrue(score >= 0);
            Assert.IsTrue(score <= 1);
        }

        // Compare two JSON structures and ensure entity and intent scores are within delta
        public static bool WithinDelta(JToken expectedToken, JToken actualToken, double delta, bool compare = false)
        {
            var withinDelta = true;
            if (expectedToken.Type == JTokenType.Object && actualToken.Type == JTokenType.Object)
            {
                var expected = (JObject)expectedToken;
                var actual = (JObject)actualToken;
                withinDelta = expected.Count == actual.Count;
                if (!withinDelta)
                {
                    // Try copying extra V2/V3 information
                    foreach (var prop in _mismatches)
                    {
                        if (actual[prop] == null && expected[prop] != null)
                        {
                            actual.Add(prop, expected[prop]);
                        }
                    }

                    withinDelta = expected.Count == actual.Count;

                    // Order alphabetically
                    var copy = (JObject)actual.DeepClone();
                    actual.RemoveAll();
                    foreach (var prop in from cprop in copy.Properties() orderby cprop.Name select cprop)
                    {
                        actual.Add(prop);
                    }
                }

                foreach (var property in expected)
                {
                    if (!withinDelta)
                    {
                        break;
                    }

                    withinDelta = actual.TryGetValue(property.Key, out var val2) && WithinDelta(property.Value, val2, delta, compare || property.Key == "score" || property.Key == "intents");
                }
            }
            else if (expectedToken.Type == JTokenType.Array && actualToken.Type == JTokenType.Array)
            {
                var arr1 = (JArray)expectedToken;
                var arr2 = (JArray)actualToken;
                withinDelta = arr1.Count == arr2.Count;
                for (var i = 0; withinDelta && i < arr1.Count; ++i)
                {
                    withinDelta = WithinDelta(arr1[i], arr2[i], delta) || withinDelta;
                }
            }
            else if (!expectedToken.Equals(actualToken))
            {
                if (expectedToken.Type == actualToken.Type)
                {
                    if (expectedToken.Type == JTokenType.String)
                    {
                        withinDelta = expectedToken.Value<string>().Equals(actualToken.Value<string>(), StringComparison.InvariantCultureIgnoreCase);
                    }
                    else
                    {
                        var val1 = (JValue)expectedToken;
                        var val2 = (JValue)actualToken;
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

            json[version] = new JObject(
                new JProperty("response", json["luisResult"]),
                new JProperty("options", oracle[version]?["options"]));
            json.Remove("luisResult");
            return (JObject)Utils.SortJSON(json);
        }
    }
}
