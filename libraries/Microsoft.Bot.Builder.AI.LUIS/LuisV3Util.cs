// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis
{
    // Utility functions used to extract and transform data from Luis SDK
    internal static class LuisV3Util
    {
        internal const string _metadataKey = "$instance";
        internal const string _geoV2 = "builtin.geographyV2.";
        internal static readonly HashSet<string> _dateSubtypes = new HashSet<string> { "date", "daterange", "datetime", "datetimerange", "duration", "set", "time", "timerange" };

        internal static string NormalizedIntent(string intent) => intent.Replace('.', '_').Replace(' ', '_');

        internal static IDictionary<string, IntentScore> GetIntents(JObject luisResult)
        {
            IDictionary<string, IntentScore> result = null;
            var intents = (JObject)luisResult["intents"];
            if (intents != null)
            {
                var dict = new Dictionary<string, IntentScore>();
                foreach (var intent in intents)
                {
                    dict.Add(NormalizedIntent(intent.Key), new IntentScore { Score = intent.Value["score"] == null ? 0.0 : intent.Value["score"].Value<double>() });
                }

                result = dict;
            }

            return result;
        }

        internal static string NormalizedEntity(string entity)
        {
            // Type::Role -> Role
            var type = entity.Split(':').Last();
            return type.Replace('.', '_').Replace(' ', '_');
        }

        internal static void GeographyTypes(JToken source, Dictionary<string, string> geoTypes)
        {
            if (source != null)
            {
                if (source is JObject obj)
                {
                    if (obj.TryGetValue("type", out var type) && type.Type == JTokenType.String && type.Value<string>().StartsWith(_geoV2))
                    {
                        var path = type.Path.Replace(_metadataKey + ".", string.Empty);
                        path = path.Substring(0, path.LastIndexOf('.'));
                        geoTypes.Add(path, type.Value<string>().Substring(_geoV2.Length));
                    }
                    else
                    {
                        foreach (var property in obj.Properties())
                        {
                            GeographyTypes(property.Value, geoTypes);
                        }
                    }
                }
                else if (source is JArray arr)
                {
                    foreach (var elt in arr)
                    {
                        GeographyTypes(elt, geoTypes);
                    }
                }
            }
        }

        internal static JToken MapProperties(JToken source, bool inInstance, Dictionary<string, string> geoTypes)
        {
            var result = source;
            if (source is JObject obj)
            {
                var nobj = new JObject();

                // Fix datetime by reverting to simple timex
                if (!inInstance && obj.TryGetValue("type", out var type) && type.Type == JTokenType.String && _dateSubtypes.Contains(type.Value<string>()))
                {
                    var timexs = obj["values"];
                    var arr = new JArray();
                    if (timexs != null)
                    {
                        var unique = new HashSet<string>();
                        foreach (var elt in timexs)
                        {
                            unique.Add(elt["timex"]?.Value<string>());
                        }

                        foreach (var timex in unique)
                        {
                            arr.Add(timex);
                        }

                        nobj["timex"] = arr;
                    }

                    nobj["type"] = type;
                }
                else
                {
                    // Map or remove properties
                    foreach (var property in obj.Properties())
                    {
                        var name = NormalizedEntity(property.Name);
                        var isObj = property.Value.Type == JTokenType.Object;
                        var isArr = property.Value.Type == JTokenType.Array;
                        var isStr = property.Value.Type == JTokenType.String;
                        var isInt = property.Value.Type == JTokenType.Integer;
                        var val = MapProperties(property.Value, inInstance || property.Name == _metadataKey, geoTypes);
                        if (name == "datetime" && isArr)
                        {
                            nobj.Add("datetimeV1", val);
                        }
                        else if (name == "datetimeV2" && isArr)
                        {
                            nobj.Add("datetime", val);
                        }
                        else if (inInstance)
                        {
                            // Correct $instance issues
                            if (name == "length" && isInt)
                            {
                                nobj.Add("endIndex", property.Value.Value<int>() + property.Parent["startIndex"].Value<int>());
                            }
                            else if (!((isStr && name == "modelType") ||
                                       (isInt && name == "modelTypeId") ||
                                       (isArr && name == "recognitionSources") ||
                                       (isStr && name == "role")))
                            {
                                nobj.Add(name, val);
                            }
                        }
                        else
                        {
                            // Correct non-$instance values
                            if (name == "unit" && isStr)
                            {
                                nobj.Add("units", val);
                            }
                            else
                            {
                                nobj.Add(name, val);
                            }
                        }
                    }
                }

                result = nobj;
            }
            else if (source is JArray arr)
            {
                var narr = new JArray();
                foreach (var elt in arr)
                {
                    if (!inInstance && geoTypes.TryGetValue(elt.Path, out var geoType))
                    {
                        narr.Add(new JObject(new JProperty("location", elt.Value<string>()), new JProperty("type", geoType)));
                    }
                    else
                    {
                        narr.Add(MapProperties(elt, inInstance, geoTypes));
                    }
                }

                result = narr;
            }

            return result;
        }

        internal static JObject ExtractEntitiesAndMetadata(JObject prediction)
        {
            var entities = (JObject)JObject.FromObject(prediction["entities"]);
            var geoTypes = new Dictionary<string, string>();
            GeographyTypes(entities, geoTypes);
            return (JObject)MapProperties(entities, false, geoTypes);
        }

        internal static void AddProperties(JObject luis, RecognizerResult result)
        {
            var sentiment = luis["sentiment"];
            if (luis["sentiment"] != null)
            {
                result.Properties.Add("sentiment", new JObject(
                    new JProperty("label", sentiment["label"]),
                    new JProperty("score", sentiment["score"])));
            }
        }
    }
}
