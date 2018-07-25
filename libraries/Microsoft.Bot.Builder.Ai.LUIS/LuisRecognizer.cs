// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Ai.Luis
{
    /// <inheritdoc />
    /// <summary>
    /// A LUIS based implementation of IRecognizer.
    /// </summary>
    public class LuisRecognizer : IRecognizer
    {
        /// <summary>
        /// The value type for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceType = "https://www.luis.ai/schemas/trace";

        /// <summary>
        /// The context label for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceLabel = "Luis Trace";
        private const string _metadataKey = "$instance";
        private readonly LuisRuntimeAPI _runtime;
        private readonly LuisApplication _application;
        private readonly LuisPredictionOptions _options;
        private readonly bool _includeApiResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        /// </summary>
        /// <param name="application">The LUIS _application to use to recognize text.</param>
        /// <param name="predictionOptions">The LUIS prediction options to use.</param>
        /// <param name="includeApiResults">TRUE to include raw LUIS API response.</param>
        public LuisRecognizer(LuisApplication application, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false)
        {
            _runtime = new LuisRuntimeAPI(new ApiKeyServiceClientCredentials(application.EndpointKey))
            {
                AzureRegion = (AzureRegions)Enum.Parse(typeof(AzureRegions), application.AzureRegion),
            };
            _application = application;
            _options = predictionOptions ?? new LuisPredictionOptions();
            _includeApiResults = includeApiResults;
        }

        /// <inheritdoc />
        public async Task<RecognizerResult> RecognizeAsync(ITurnContext context, CancellationToken ct)
            => await RecognizeInternalAsync(context, ct).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<T> RecognizeAsync<T>(ITurnContext context, CancellationToken ct)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(context, ct).ConfigureAwait(false));
            return result;
        }

        private static string NormalizedIntent(string intent) => intent.Replace('.', '_').Replace(' ', '_');

        private static IDictionary<string, IntentScore> GetIntents(LuisResult luisResult)
            => luisResult.Intents != null ?
                luisResult.Intents.ToDictionary(
                    i => NormalizedIntent(i.Intent),
                    i => new IntentScore { Score = i.Score ?? 0 })
                    :
                new Dictionary<string, IntentScore>()
                {
                    {
                        NormalizedIntent(luisResult.TopScoringIntent.Intent),
                        new IntentScore() { Score = luisResult.TopScoringIntent.Score ?? 0 }
                    },
                };

        private static JObject ExtractEntitiesAndMetadata(IList<EntityModel> entities, IList<CompositeEntityModel> compositeEntities, bool verbose)
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

        private static JToken ExtractEntityValue(EntityModel entity)
        {
#pragma warning disable IDE0007 // Use implicit type
            if (entity.AdditionalProperties == null || !entity.AdditionalProperties.TryGetValue("resolution", out dynamic resolution))
#pragma warning restore IDE0007 // Use implicit type
            {
                return entity.Entity;
            }

            if (entity.Type.StartsWith("builtin.datetimeV2."))
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
                        return resolution.value ?? JArray.FromObject(resolution.values);
                }
            }
        }

        private static JObject ExtractEntityMetadata(EntityModel entity)
        {
            dynamic obj = JObject.FromObject(new
            {
                startIndex = (int)entity.StartIndex,
                endIndex = (int)entity.EndIndex + 1,
                text = entity.Entity,
            });
            if (entity.AdditionalProperties != null && entity.AdditionalProperties.TryGetValue("score", out var score))
            {
                obj.score = (double)score;
            }

            return obj;
        }

        private static string ExtractNormalizedEntityName(EntityModel entity)
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

        private static IList<EntityModel> PopulateCompositeEntityModel(CompositeEntityModel compositeEntity, IList<EntityModel> entities, JObject entitiesAndMetadata, bool verbose)
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

            AddProperty(entitiesAndMetadata, compositeEntity.ParentType, childrenEntites);
            if (verbose)
            {
                AddProperty((JObject)entitiesAndMetadata[_metadataKey], compositeEntity.ParentType, childrenEntitiesMetadata);
            }

            // filter entities that were covered by this composite entity
            return entities.Except(coveredSet).ToList();
        }

        private static bool CompositeContainsEntity(EntityModel compositeEntityMetadata, EntityModel entity)
            => entity.StartIndex >= compositeEntityMetadata.StartIndex &&
                   entity.EndIndex <= compositeEntityMetadata.EndIndex;

        /// <summary>
        /// If a property doesn't exist add it to a new array, otherwise append it to the existing array.
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

        private static void AddProperties(LuisResult luis, RecognizerResult result)
        {
            if (luis.SentimentAnalysis != null)
            {
                result.Properties.Add("sentiment", new JObject(
                    new JProperty("label", luis.SentimentAnalysis.Label),
                    new JProperty("score", luis.SentimentAnalysis.Score)));
            }
        }

        private async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext context, CancellationToken ct)
        {
            BotAssert.ContextNotNull(context);

            if (context.Activity.Type != ActivityTypes.Message)
            {
                return null;
            }

            var utterance = context.Activity?.AsMessageActivity()?.Text;

            if (string.IsNullOrWhiteSpace(utterance))
            {
                throw new ArgumentNullException(nameof(utterance));
            }

            var luisResult = await _runtime.Prediction.ResolveAsync(
                _application.ApplicationId,
                utterance,
                timezoneOffset: _options.TimezoneOffset,
                verbose: _options.Verbose,
                staging: _options.Staging,
                spellCheck: _options.SpellCheck,
                bingSpellCheckSubscriptionKey: _options.BingSpellCheckSubscriptionKey,
                log: _options.Log,
                cancellationToken: ct).ConfigureAwait(false);

            var recognizerResult = new RecognizerResult
            {
                Text = utterance,
                AlteredText = luisResult.AlteredQuery,
                Intents = GetIntents(luisResult),
                Entities = ExtractEntitiesAndMetadata(luisResult.Entities, luisResult.CompositeEntities, _options.IncludeInstanceData ?? true),
            };
            AddProperties(luisResult, recognizerResult);
            if (_includeApiResults)
            {
                recognizerResult.Properties.Add("luisResult", luisResult);
            }

            var traceInfo = JObject.FromObject(
                new
                {
                    recognizerResult,
                    luisModel = new
                    {
                        ModelID = _application.ApplicationId,
                    },
                    luisOptions = _options,
                    luisResult,
                });

            await context.TraceActivityAsync("LuisRecognizer", traceInfo, LuisTraceType, LuisTraceLabel, ct).ConfigureAwait(false);
            return recognizerResult;
        }
    }
}
