// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <inheritdoc />
    /// <summary>
    /// A LUIS based implementation of IRecognizer.
    /// </summary>
    public class LuisRecognizer : IRecognizer
    {
        private const string MetadataKey = "$instance";
        private readonly LuisRuntimeAPI luisService;
        private readonly LuisPredictionOptions luisOptions;
        private readonly ILuisRecognizerOptions luisRecognizerOptions;

        /// <summary> 
        /// Creates a new <see cref="LuisRecognizer"/> object. 
        /// </summary> 
        /// <param name="luisModel">The LUIS model to use to recognize text.</param> 
        /// <param name="luisRecognizerOptions">The LUIS recognizer options to use.</param> 
        /// <param name="options">The LUIS request options to use.</param> 
        public LuisRecognizer(LuisApplication luisModel, ILuisRecognizerOptions luisRecognizerOptions = null, LuisPredictionOptions options = null)
        {
            luisService = new LuisRuntimeAPI(new );
            luisOptions = options ?? new LuisOptions();
            this.luisRecognizerOptions = luisRecognizerOptions ?? new LuisRecognizerOptions { Verbose = true };
        }

        /// <inheritdoc />
        public async Task<RecognizerResult> Recognize(string utterance, CancellationToken ct)
        {
            return (await RecognizeInternal(utterance, ct).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public async Task<T> Recognize<T>(string utterance, CancellationToken ct)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternal(utterance, ct).ConfigureAwait(false));
            return result;
        }

        private Task<RecognizerResult> RecognizeInternal(string utterance, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(utterance))
                throw new ArgumentNullException(nameof(utterance));

            var luisRequest = new LuisRequest(utterance);
            luisOptions.Apply(luisRequest);
            return Recognize(luisRequest, ct, luisRecognizerOptions.Verbose);
        }

        private async Task<RecognizerResult> Recognize(LuisRequest request, CancellationToken ct, bool verbose)
        {
            var luisResult = await luisService.QueryAsync(request, ct).ConfigureAwait(false);
            var recognizerResult = new RecognizerResult
            {
                Text = request.Query,
                AlteredText = luisResult.AlteredQuery,
                Intents = GetIntents(luisResult),
                Entities = ExtractEntitiesAndMetadata(luisResult.Entities, luisResult.CompositeEntities, verbose),
            };
            recognizerResult.Properties.Add("luisResult", luisResult);
            return recognizerResult;
        }

        private static string NormalizedIntent(string intent)
        {
            return intent.Replace('.', '_').Replace(' ', '_');
        }

        private static JObject GetIntents(LuisResult luisResult)
        {
            return luisResult.Intents != null ?
                JObject.FromObject(luisResult.Intents.ToDictionary(
                    i => NormalizedIntent(i.Intent),
                    i => new JObject(new JProperty("score", i.Score ?? 0)))) :
                new JObject { [NormalizedIntent(luisResult.TopScoringIntent.Intent)] = new JProperty("score", luisResult.TopScoringIntent.Score ?? 0) };
        }

        private static JObject ExtractEntitiesAndMetadata(IList<EntityRecommendation> entities, IList<CompositeEntity> compositeEntities, bool verbose)
        {
            var entitiesAndMetadata = new JObject();
            if (verbose)
            {
                entitiesAndMetadata[MetadataKey] = new JObject();
            }
            var compositeEntityTypes = new HashSet<string>();

            // We start by populating composite entities so that entities covered by them are removed from the entities list
            if (compositeEntities != null && compositeEntities.Any())
            {
                compositeEntityTypes = new HashSet<string>(compositeEntities.Select(ce => ce.ParentType));
                entities = compositeEntities.Aggregate(entities, (current, compositeEntity) => PopulateCompositeEntity(compositeEntity, current, entitiesAndMetadata, verbose));
            }

            foreach (var entity in entities)
            {
                // we'll address composite entities separately
                if (compositeEntityTypes.Contains(entity.Type))
                    continue;

                AddProperty(entitiesAndMetadata, ExtractNormalizedEntityName(entity), ExtractEntityValue(entity));

                if (verbose)
                {
                    AddProperty((JObject)entitiesAndMetadata[MetadataKey], ExtractNormalizedEntityName(entity), ExtractEntityMetadata(entity));
                }
            }

            return entitiesAndMetadata;
        }

        private static JToken Number(dynamic value)
        {
            if (value == null)
            {
                return null;
            }
            return long.TryParse((string)value, out var longVal) ?
                            new JValue(longVal) :
                            new JValue(double.Parse((string)value));
        }

        private static JToken ExtractEntityValue(EntityRecommendation entity)
        {
            if (entity.Resolution == null)
                return entity.Entity;

            if (entity.Type.StartsWith("builtin.datetimeV2."))
            {
                if (entity.Resolution?.Values == null || entity.Resolution.Values.Count == 0)
                    return JArray.FromObject(entity.Resolution);

                var resolutionValues = (IEnumerable<object>)entity.Resolution["values"];
                var type = ((IDictionary<string, object>)(resolutionValues.First()))["type"];
                var timexes = resolutionValues.Select(val => ((IDictionary<string, object>)val)["timex"]);
                var distinctTimexes = timexes.Distinct();
                return new JObject(new JProperty("type", type), new JProperty("timex", JArray.FromObject(distinctTimexes)));
            }
            else
            {
                var resolution = entity.Resolution;
                switch (entity.Type)
                {
                    case "builtin.number":
                    case "builtin.ordinal": return Number(resolution["value"]);
                    case "builtin.percentage":
                        {
                            var svalue = (string)resolution["value"];
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
                            var units = (string)resolution["unit"];
                            var val = Number(resolution["value"]);
                            var obj = new JObject();
                            if (val != null)
                            {
                                obj.Add("number", val);
                            }
                            obj.Add("units", units);
                            return obj;
                        }
                    default:
                        return entity.Resolution.Count > 1 ?
                            JObject.FromObject(entity.Resolution) :
                            entity.Resolution.ContainsKey("value") ?
                                (JToken)new JValue(entity.Resolution["value"]) :
                                JArray.FromObject(entity.Resolution["values"]);
                }
            }
        }

        private static JObject ExtractEntityMetadata(EntityRecommendation entity)
        {
            var obj = JObject.FromObject(new
            {
                startIndex = entity.StartIndex,
                endIndex = entity.EndIndex + 1,
                text = entity.Entity
            });
            if (entity.Score.HasValue)
            {
                obj["score"] = entity.Score;
            }
            return obj;
        }

        private static string ExtractNormalizedEntityName(EntityRecommendation entity)
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
            if (!string.IsNullOrWhiteSpace(entity.Role ))
            {
                type = entity.Role;
            }
            return type.Replace('.', '_').Replace(' ', '_');
        }

        private static IList<EntityRecommendation> PopulateCompositeEntity(CompositeEntity compositeEntity, IList<EntityRecommendation> entities, JObject entitiesAndMetadata, bool verbose)
        {
            var childrenEntites = new JObject();
            var childrenEntitiesMetadata = new JObject();
            if (verbose)
            {
                childrenEntites[MetadataKey] = new JObject();
            }

            // This is now implemented as O(n^2) search and can be reduced to O(2n) using a map as an optimization if n grows
            var compositeEntityMetadata = entities.FirstOrDefault(e => e.Type == compositeEntity.ParentType && e.Entity == compositeEntity.Value);

            // This is an error case and should not happen in theory
            if (compositeEntityMetadata == null)
                return entities;

            if (verbose)
            {
                childrenEntitiesMetadata = ExtractEntityMetadata(compositeEntityMetadata);
                childrenEntites[MetadataKey] = new JObject();
            }

            var coveredSet = new HashSet<EntityRecommendation>();
            foreach (var child in compositeEntity.Children)
            {
                foreach (var entity in entities)
                {
                    // We already covered this entity
                    if (coveredSet.Contains(entity))
                        continue;

                    // This entity doesn't belong to this composite entity
                    if (child.Type != entity.Type || !CompositeContainsEntity(compositeEntityMetadata, entity))
                        continue;

                    // Add to the set to ensure that we don't consider the same child entity more than once per composite
                    coveredSet.Add(entity);
                    AddProperty(childrenEntites, ExtractNormalizedEntityName(entity), ExtractEntityValue(entity));

                    if (verbose)
                    {
                        AddProperty((JObject)childrenEntites[MetadataKey], ExtractNormalizedEntityName(entity), ExtractEntityMetadata(entity));
                    }
                }
            }

            AddProperty(entitiesAndMetadata, compositeEntity.ParentType, childrenEntites);
            if (verbose)
            {
                AddProperty((JObject)entitiesAndMetadata[MetadataKey], compositeEntity.ParentType, childrenEntitiesMetadata);
            }

            // filter entities that were covered by this composite entity
            return entities.Except(coveredSet).ToList();
        }

        private static bool CompositeContainsEntity(EntityRecommendation compositeEntityMetadata, EntityRecommendation entity)
        {
            return entity.StartIndex >= compositeEntityMetadata.StartIndex &&
                   entity.EndIndex <= compositeEntityMetadata.EndIndex;
        }

        /// <summary>
        /// If a property doesn't exist add it to a new array, otherwise append it to the existing array
        /// </summary>
        private static void AddProperty(JObject obj, string key, JToken value)
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
}
