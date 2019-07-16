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

        /* TODO: Remove
        internal static JObject ExtractEntitiesAndMetadata(IList<EntityModel> entities, IList<CompositeEntityModel> compositeEntities, bool verbose)
        {
            var entitiesAndMetadata = new JObject();
            if (verbose)
            {
                entitiesAndMetadata[_metadataKey] = new JObject();
            }

            var compositeEntityTypes = new HashSet<string>();

            // We start by populating composite entities so that entities covered by them are removed from the entities list
            if (compositeEntities != null && compositeEntities.Any())
            {
                compositeEntityTypes = new HashSet<string>(compositeEntities.Select(ce => ce.ParentType));
                entities = compositeEntities.Aggregate(entities, (current, compositeEntity) => PopulateCompositeEntityModel(compositeEntity, current, entitiesAndMetadata, verbose));
            }

            foreach (var entity in entities)
            {
                // we'll address composite entities separately
                if (compositeEntityTypes.Contains(entity.Type))
                {
                    continue;
                }

                AddProperty(entitiesAndMetadata, ExtractNormalizedEntityName(entity), ExtractEntityValue(entity));

                if (verbose)
                {
                    AddProperty((JObject)entitiesAndMetadata[_metadataKey], ExtractNormalizedEntityName(entity), ExtractEntityMetadata(entity));
                }
            }

            return entitiesAndMetadata;
        }

        internal static JToken Number(dynamic value)
        {
            if (value == null)
            {
                return null;
            }

            return long.TryParse((string)value, out var longVal) ?
                            new JValue(longVal) :
                            new JValue(double.Parse((string)value));
        }

        internal static JToken ExtractEntityValue(EntityModel entity)
        {
        #pragma warning disable IDE0007 // Use implicit type
            if (entity.AdditionalProperties == null || !entity.AdditionalProperties.TryGetValue("resolution", out dynamic resolution))
        #pragma warning restore IDE0007 // Use implicit type
            {
                return entity.Entity;
            }

            if (entity.Type.StartsWith("builtin.datetime."))
            {
                return JObject.FromObject(resolution);
            }
            else if (entity.Type.StartsWith("builtin.datetimeV2."))
            {
                if (resolution.values == null || resolution.values.Count == 0)
                {
                    return JArray.FromObject(resolution);
                }

                var resolutionValues = (IEnumerable<dynamic>)resolution.values;
                var type = resolution.values[0].type;
                var timexes = resolutionValues.Select(val => val.timex);
                var distinctTimexes = timexes.Distinct();
                return new JObject(new JProperty("type", type), new JProperty("timex", JArray.FromObject(distinctTimexes)));
            }
            else
            {
                switch (entity.Type)
                {
                    case "builtin.number":
                    case "builtin.ordinal": return Number(resolution.value);
                    case "builtin.percentage":
                        {
                            var svalue = (string)resolution.value;
                            if (svalue.EndsWith("%"))
                            {
                                svalue = svalue.Substring(0, svalue.Length - 1);
                            }

                            return Number(svalue);
                        }

                    case "builtin.age":
                    case "builtin.dimension":
                    case "builtin.currency":
                    case "builtin.temperature":
                        {
                            var units = (string)resolution.unit;
                            var val = Number(resolution.value);
                            var obj = new JObject();
                            if (val != null)
                            {
                                obj.Add("number", val);
                            }

                            obj.Add("units", units);
                            return obj;
                        }

                    default:
                        return resolution.value ?? (resolution.values != null ? JArray.FromObject(resolution.values) : resolution);
                }
            }
        }

        internal static JObject ExtractEntityMetadata(EntityModel entity)
        {
            dynamic obj = JObject.FromObject(new
            {
                startIndex = (int)entity.StartIndex,
                endIndex = (int)entity.EndIndex + 1,
                text = entity.Entity,
                type = entity.Type,
            });
            if (entity.AdditionalProperties != null)
            {
                if (entity.AdditionalProperties.TryGetValue("score", out var score))
                {
                    obj.score = (double)score;
                }

        #pragma warning disable IDE0007 // Use implicit type
                if (entity.AdditionalProperties.TryGetValue("resolution", out dynamic resolution) && resolution.subtype != null)
        #pragma warning restore IDE0007 // Use implicit type
                {
                    obj.subtype = resolution.subtype;
                }
            }

            return obj;
        }

        internal static string ExtractNormalizedEntityName(EntityModel entity)
        {
            // Type::Role -> Role
            var type = entity.Type.Split(':').Last();
            if (type.StartsWith("builtin.datetimeV2."))
            {
                type = "datetime";
            }

            if (type.StartsWith("builtin.currency"))
            {
                type = "money";
            }

            if (type.StartsWith("builtin."))
            {
                type = type.Substring(8);
            }

            var role = entity.AdditionalProperties != null && entity.AdditionalProperties.ContainsKey("role") ? (string)entity.AdditionalProperties["role"] : string.Empty;
            if (!string.IsNullOrWhiteSpace(role))
            {
                type = role;
            }

            return type.Replace('.', '_').Replace(' ', '_');
        }

        internal static IList<EntityModel> PopulateCompositeEntityModel(CompositeEntityModel compositeEntity, IList<EntityModel> entities, JObject entitiesAndMetadata, bool verbose)
        {
            var childrenEntites = new JObject();
            var childrenEntitiesMetadata = new JObject();
            if (verbose)
            {
                childrenEntites[_metadataKey] = new JObject();
            }

            // This is now implemented as O(n^2) search and can be reduced to O(2n) using a map as an optimization if n grows
            var compositeEntityMetadata = entities.FirstOrDefault(e => e.Type == compositeEntity.ParentType && e.Entity == compositeEntity.Value);

            // This is an error case and should not happen in theory
            if (compositeEntityMetadata == null)
            {
                return entities;
            }

            if (verbose)
            {
                childrenEntitiesMetadata = ExtractEntityMetadata(compositeEntityMetadata);
                childrenEntites[_metadataKey] = new JObject();
            }

            var coveredSet = new HashSet<EntityModel>();
            foreach (var child in compositeEntity.Children)
            {
                foreach (var entity in entities)
                {
                    // We already covered this entity
                    if (coveredSet.Contains(entity))
                    {
                        continue;
                    }

                    // This entity doesn't belong to this composite entity
                    if (child.Type != entity.Type || !CompositeContainsEntity(compositeEntityMetadata, entity))
                    {
                        continue;
                    }

                    // Add to the set to ensure that we don't consider the same child entity more than once per composite
                    coveredSet.Add(entity);
                    AddProperty(childrenEntites, ExtractNormalizedEntityName(entity), ExtractEntityValue(entity));

                    if (verbose)
                    {
                        AddProperty((JObject)childrenEntites[_metadataKey], ExtractNormalizedEntityName(entity), ExtractEntityMetadata(entity));
                    }
                }
            }

            AddProperty(entitiesAndMetadata, ExtractNormalizedEntityName(compositeEntityMetadata), childrenEntites);
            if (verbose)
            {
                AddProperty((JObject)entitiesAndMetadata[_metadataKey], ExtractNormalizedEntityName(compositeEntityMetadata), childrenEntitiesMetadata);
            }

            // filter entities that were covered by this composite entity
            return entities.Except(coveredSet).ToList();
        }

        internal static bool CompositeContainsEntity(EntityModel compositeEntityMetadata, EntityModel entity)
            => entity.StartIndex >= compositeEntityMetadata.StartIndex &&
                   entity.EndIndex <= compositeEntityMetadata.EndIndex;

        /// <summary>
        /// If a property doesn't exist add it to a new array, otherwise append it to the existing array.
        /// </summary>
        internal static void AddProperty(JObject obj, string key, JToken value)
        {
            if (value != null)
            {
                if (((IDictionary<string, JToken>)obj).ContainsKey(key))
                {
                    ((JArray)obj[key]).Add(value);
                }
                else
                {
                    obj[key] = new JArray(value);
                }
            }
        }
        */

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
