// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS;
using Microsoft.Cognitive.LUIS.Models;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.LUIS
{
    /// <inheritdoc />
    /// <summary>
    /// A Luis based implementation of IRecognizer
    /// </summary>
    public class LuisRecognizer : IRecognizer
    {
        private readonly LuisService _luisService;
        private readonly ILuisOptions _luisOptions;
        private readonly ILuisRecognizerOptions _luisRecognizerOptions;
        private const string MetadataKey = "$instance";

        public LuisRecognizer(ILuisModel luisModel, ILuisRecognizerOptions luisRecognizerOptions = null, ILuisOptions options = null)
        {
            _luisService = new LuisService(luisModel);
            _luisOptions = options ?? new LuisRequest();
            _luisRecognizerOptions = luisRecognizerOptions ?? new LuisRecognizerOptions { Verbose = true };
        }

        /// <inheritdoc />
        public Task<RecognizerResult> Recognize(string utterance, CancellationToken ct)
        {
            if(string.IsNullOrEmpty(utterance))
                throw new ArgumentNullException(nameof(utterance));

            var luisRequest = new LuisRequest(utterance);
            _luisOptions.Apply(luisRequest);
            return Recognize(luisRequest, ct, _luisRecognizerOptions.Verbose);
        }

        private async Task<RecognizerResult> Recognize(LuisRequest request, CancellationToken ct, bool verbose)
        {
            var luisResult = await _luisService.QueryAsync(request, ct).ConfigureAwait(false);

            var recognizerResult = new RecognizerResult
            {
                Text = request.Query,
                AlteredText = luisResult.AlteredQuery,
                Intents = GetIntents(luisResult),
                Entities = GetEntitiesAndMetadata(luisResult.Entities, luisResult.CompositeEntities, verbose)
            };

            return recognizerResult;
        }

        private static JObject GetIntents(LuisResult luisResult)
        {
            return luisResult.Intents != null ?
                JObject.FromObject(luisResult.Intents.ToDictionary(i => i.Intent, i => i.Score ?? 0)) :
                new JObject { [luisResult.TopScoringIntent.Intent] = luisResult.TopScoringIntent.Score ?? 0 };
        }

        private static JObject GetEntitiesAndMetadata(IList<EntityRecommendation> entities, IList<CompositeEntity> compositeEntities, bool verbose)
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

                AddProperty(entitiesAndMetadata, GetNormalizedEntityType(entity), GetEntityValue(entity));

                if (verbose)
                {
                    AddProperty((JObject) entitiesAndMetadata[MetadataKey], GetNormalizedEntityType(entity), GetEntityMetadata(entity));
                }
            }

            return entitiesAndMetadata;
        }

        private static JToken GetEntityValue(EntityRecommendation entity)
        {
            if (entity.Resolution == null)
                return entity.Entity;

            if (entity.Type.StartsWith("builtin.datetimeV2."))
            {
                if (entity.Resolution?.Values == null || entity.Resolution.Values.Count == 0)
                    return JArray.FromObject(entity.Resolution);

                var resolutionValues = (IEnumerable<object>)entity.Resolution.Values.First();
                var timexes = resolutionValues.Select(value => ((IDictionary<string, object>) value)["timex"]);
                var distinctTimexes = timexes.Distinct();
                return JArray.FromObject(distinctTimexes);
            }
            
            if (entity.Type.StartsWith("builtin.number") || entity.Type.StartsWith("builtin.ordinal"))
            {
                var value = (string) entity.Resolution.Values.First();
                return long.TryParse(value, out var longVal) ?
                            new JValue(longVal) :
                            new JValue(double.Parse(value));
            }

            return entity.Resolution.Count > 1 ? 
                JObject.FromObject(entity.Resolution) : 
                entity.Resolution.ContainsKey("value") ?
                    (JToken) JObject.FromObject(entity.Resolution["value"]) :
                    JArray.FromObject(entity.Resolution["values"]);
        }

        private static JObject GetEntityMetadata(EntityRecommendation entity)
        {
            return JObject.FromObject(new
            {
                startIndex = entity.StartIndex,
                endIndex = entity.EndIndex,
                text = entity.Entity,
                score = entity.Score
            });
        }

        private static string GetNormalizedEntityType(EntityRecommendation entity)
        {
            return Regex.Replace(entity.Type, "\\.", "_");
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
                childrenEntitiesMetadata = GetEntityMetadata(compositeEntityMetadata);
                childrenEntites[MetadataKey] = new JObject();
            }
            
            var coveredSet = new HashSet<EntityRecommendation>();
            foreach (var child in compositeEntity.Children)
            {
                foreach(var entity in entities)
                {
                    // We already covered this entity
                    if (coveredSet.Contains(entity))
                        continue;

                    // This entity doesn't belong to this composite entity
                    if (child.Type != entity.Type || !CompositeContainsEntity(compositeEntityMetadata, entity))
                        continue;

                    // Add to the set to ensure that we don't consider the same child entity more than once per composite
                    coveredSet.Add(entity);
                    AddProperty(childrenEntites, GetNormalizedEntityType(entity), GetEntityValue(entity));

                    if (verbose)
                    {
                        AddProperty((JObject)childrenEntites[MetadataKey], GetNormalizedEntityType(entity), GetEntityMetadata(entity));
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
                ((JArray) obj[key]).Add(value);
            }
            else
            {
                obj[key] = new JArray(value);
            }
        }
    }
}
