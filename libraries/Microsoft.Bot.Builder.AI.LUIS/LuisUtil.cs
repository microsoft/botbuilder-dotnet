// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis
{
    // Utility functions used to extract and transform data from Luis SDK
    internal static class LuisUtil
    {
        internal const string _metadataKey = "$instance";

        internal static string NormalizedIntent(string intent) => intent.Replace('.', '_').Replace(' ', '_');

        internal static IDictionary<string, IntentScore> GetIntents(LuisResult luisResult)
        {
            if (luisResult.Intents != null)
            {
                return luisResult.Intents.ToDictionary(
                    i => NormalizedIntent(i.Intent),
                    i => new IntentScore { Score = i.Score ?? 0 });
            }
            else
            {
                return new Dictionary<string, IntentScore>()
                {
                    {
                        NormalizedIntent(luisResult.TopScoringIntent.Intent),
                        new IntentScore() { Score = luisResult.TopScoringIntent.Score ?? 0 }
                    },
                };
            }
        }

        internal static JObject ExtractEntitiesAndMetadata(IList<EntityModel> entities, IList<CompositeEntityModel> compositeEntities, bool verbose, string utterance)
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
                entities = compositeEntities.Aggregate(entities, (current, compositeEntity) => PopulateCompositeEntityModel(compositeEntity, current, entitiesAndMetadata, verbose, utterance));
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
                    AddProperty((JObject)entitiesAndMetadata[_metadataKey], ExtractNormalizedEntityName(entity), ExtractEntityMetadata(entity, utterance));
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
                            new JValue(double.Parse((string)value, CultureInfo.InvariantCulture));
        }

        internal static JToken ExtractEntityValue(EntityModel entity)
        {
            if (entity.Type.StartsWith("builtin.geographyV2.", StringComparison.Ordinal))
            {
                var subtype = entity.Type.Substring(20);
                return new JObject(
                    new JProperty("type", subtype),
                    new JProperty("location", entity.Entity));
            }

            if (entity.AdditionalProperties == null || !entity.AdditionalProperties.TryGetValue("resolution", out dynamic resolution))
            {
                return entity.Entity;
            }

            if (entity.Type.StartsWith("builtin.datetime.", StringComparison.Ordinal))
            {
                return JObject.FromObject(resolution);
            }

            if (entity.Type.StartsWith("builtin.datetimeV2.", StringComparison.Ordinal))
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

            if (entity.Type.StartsWith("builtin.ordinalV2", StringComparison.Ordinal))
            {
                return new JObject(
                    new JProperty("relativeTo", resolution.relativeTo),
                    new JProperty("offset", Number(resolution.offset)));
            }

            switch (entity.Type)
            {
                case "builtin.number":
                case "builtin.ordinal": return Number(resolution.value);
                case "builtin.percentage":
                {
                    var svalue = (string)resolution.value;
                    if (svalue.EndsWith("%", StringComparison.Ordinal))
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

        internal static JObject ExtractEntityMetadata(EntityModel entity, string utterance)
        {
            var start = entity.StartIndex;
            var end = entity.EndIndex + 1;
            dynamic obj = JObject.FromObject(new
            {
                startIndex = start,
                endIndex = end,
                text = GetEntityText(entity, utterance, start, end),
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

        internal static string GetEntityText(EntityModel entity, string utterance, int start, int end)
        {
            string result;
            var entitySize = end - start;

            if (entity.Entity.Length == entitySize)
            {
                result = entity.Entity;
            }
            else if (utterance.Length <= entitySize)
            {
                result = utterance;
            }
            else
            {
                result = utterance.Substring(start, entitySize);
            }

            return result;
        }

        internal static string ExtractNormalizedEntityName(EntityModel entity)
        {
            // Type::Role -> Role
            var type = entity.Type.Split(':').Last();
            if (type.StartsWith("builtin.datetimeV2.", StringComparison.Ordinal))
            {
                type = "datetime";
            }
            else if (type.StartsWith("builtin.currency", StringComparison.Ordinal))
            {
                type = "money";
            }
            else if (type.StartsWith("builtin.geographyV2", StringComparison.Ordinal))
            {
                type = "geographyV2";
            }
            else if (type.StartsWith("builtin.ordinalV2", StringComparison.Ordinal))
            {
                type = "ordinalV2";
            }
            else if (type.StartsWith("builtin.", StringComparison.Ordinal))
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

        internal static IList<EntityModel> PopulateCompositeEntityModel(CompositeEntityModel compositeEntity, IList<EntityModel> entities, JObject entitiesAndMetadata, bool verbose, string utterance)
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
                childrenEntitiesMetadata = ExtractEntityMetadata(compositeEntityMetadata, utterance);
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
                    if (child.Type != entity.Type || !CompositeContainsEntity(compositeEntityMetadata, entity) || child.Value != entity.Entity)
                    {
                        continue;
                    }

                    // Add to the set to ensure that we don't consider the same child entity more than once per composite
                    coveredSet.Add(entity);
                    AddProperty(childrenEntites, ExtractNormalizedEntityName(entity), ExtractEntityValue(entity));

                    if (verbose)
                    {
                        AddProperty((JObject)childrenEntites[_metadataKey], ExtractNormalizedEntityName(entity), ExtractEntityMetadata(entity, utterance));
                    }

                    break;
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
        /// <param name="obj">Object in which the property will be added.</param>
        /// <param name="key">Key of the property.</param>
        /// <param name="value">Value for the property.</param>
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

        internal static void AddProperties(LuisResult luis, RecognizerResult result)
        {
            if (luis.SentimentAnalysis != null)
            {
                result.Properties.Add("sentiment", new JObject(
                    new JProperty("label", luis.SentimentAnalysis.Label),
                    new JProperty("score", luis.SentimentAnalysis.Score)));
            }
        }
    }
}
