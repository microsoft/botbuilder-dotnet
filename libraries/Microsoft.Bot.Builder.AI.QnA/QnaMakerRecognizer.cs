// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <inheritdoc />
    /// <summary>
    /// A LUIS based implementation of <see cref="IRecognizer"/>.
    /// </summary>
    public class QnAMakerRecognizer : IRecognizer
    {
        public QnAMakerRecognizer()
        {
        }

        //        /// <summary>
        //        /// The value type for a LUIS trace activity.
        //        /// </summary>
        //        public const string LuisTraceType = "https://www.luis.ai/schemas/trace";

        //        /// <summary>
        //        /// The context label for a LUIS trace activity.
        //        /// </summary>
        //        public const string LuisTraceLabel = "Luis Trace";
        //        private const string _metadataKey = "$instance";
        //        private readonly ILUISRuntimeClient _runtime;
        //        private readonly LuisApplication _application;
        //        private readonly LuisPredictionOptions _options;
        //        private readonly bool _includeApiResults;

        //        /// <summary>
        //        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        //        /// </summary>
        //        /// <param name="application">The LUIS application to use to recognize text.</param>
        //        /// <param name="predictionOptions">(Optional) The LUIS prediction options to use.</param>
        //        /// <param name="includeApiResults">(Optional) TRUE to include raw LUIS API response.</param>
        //        /// <param name="clientHandler">(Optional) Custom handler for LUIS API calls to allow mocking.</param>
        //        public LuisRecognizer(LuisApplication application, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, HttpClientHandler clientHandler = null)
        //        {
        //            _application = application ?? throw new ArgumentNullException(nameof(application));
        //            _options = predictionOptions ?? new LuisPredictionOptions();
        //            _includeApiResults = includeApiResults;

        //            var credentials = new ApiKeyServiceClientCredentials(application.EndpointKey);
        //            var delegatingHandler = new LuisDelegatingHandler();

        //            // LUISRuntimeClient requires that we explicitly bind to the appropriate constructor.
        //            _runtime = clientHandler == null
        //                    ?
        //                new LUISRuntimeClient(credentials, delegatingHandler)
        //                    :
        //                new LUISRuntimeClient(credentials, clientHandler, delegatingHandler);

        //            _runtime.Endpoint = application.Endpoint;
        //        }

        //        /// <summary>
        //        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        //        /// </summary>
        //        /// <param name="service">The LUIS service from configuration.</param>
        //        /// <param name="predictionOptions">(Optional) The LUIS prediction options to use.</param>
        //        /// <param name="includeApiResults">(Optional) TRUE to include raw LUIS API response.</param>
        //        /// <param name="clientHandler">(Optional) Custom handler for LUIS API calls to allow mocking.</param>
        //        public LuisRecognizer(LuisService service, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, HttpClientHandler clientHandler = null)
        //            : this(new LuisApplication(service), predictionOptions, includeApiResults, clientHandler)
        //        {
        //        }

        //        /// <summary>
        //        /// Returns the name of the top scoring intent from a set of LUIS results.
        //        /// </summary>
        //        /// <param name="results">Result set to be searched.</param>
        //        /// <param name="defaultIntent">(Optional) Intent name to return should a top intent be found. Defaults to a value of "None".</param>
        //        /// <param name="minScore">(Optional) Minimum score needed for an intent to be considered as a top intent. If all intents in the set are below this threshold then the `defaultIntent` will be returned.  Defaults to a value of `0.0`.</param>
        //        /// <returns>The top scoring intent name.</returns>
        //        public static string TopIntent(RecognizerResult results, string defaultIntent = "None", double minScore = 0.0)
        //        {
        //            if (results == null)
        //            {
        //                throw new ArgumentNullException(nameof(results));
        //            }

        //            string topIntent = null;
        //            double topScore = -1.0;
        //            if (results.Intents.Count > 0)
        //            {
        //                foreach (var intent in results.Intents)
        //                {
        //                    var score = (double)intent.Value.Score;
        //                    if (score > topScore && score >= minScore)
        //                    {
        //                        topIntent = intent.Key;
        //                        topScore = score;
        //                    }
        //                }
        //            }

        //            return !string.IsNullOrEmpty(topIntent) ? topIntent : defaultIntent;
        //        }

        //        /// <inheritdoc />
        //        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        //            => await RecognizeInternalAsync(turnContext, cancellationToken).ConfigureAwait(false);

        //        /// <inheritdoc />
        //        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
        //            where T : IRecognizerConvert, new()
        //        {
        //            var result = new T();
        //            result.Convert(await RecognizeInternalAsync(turnContext, cancellationToken).ConfigureAwait(false));
        //            return result;
        //        }

        //        private static string NormalizedIntent(string intent) => intent.Replace('.', '_').Replace(' ', '_');

        //        private static IDictionary<string, IntentScore> GetIntents(LuisResult luisResult)
        //        {
        //            if (luisResult.Intents != null)
        //            {
        //                return luisResult.Intents.ToDictionary(
        //                    i => NormalizedIntent(i.Intent),
        //                    i => new IntentScore { Score = i.Score ?? 0 });
        //            }
        //            else
        //            {
        //                return new Dictionary<string, IntentScore>()
        //                {
        //                    {
        //                        NormalizedIntent(luisResult.TopScoringIntent.Intent),
        //                        new IntentScore() { Score = luisResult.TopScoringIntent.Score ?? 0 }
        //                    },
        //                };
        //            }
        //        }

        //        private static JObject ExtractEntitiesAndMetadata(IList<EntityModel> entities, IList<CompositeEntityModel> compositeEntities, bool verbose)
        //        {
        //            var entitiesAndMetadata = new JObject();
        //            if (verbose)
        //            {
        //                entitiesAndMetadata[_metadataKey] = new JObject();
        //            }

        //            var compositeEntityTypes = new HashSet<string>();

        //            // We start by populating composite entities so that entities covered by them are removed from the entities list
        //            if (compositeEntities != null && compositeEntities.Any())
        //            {
        //                compositeEntityTypes = new HashSet<string>(compositeEntities.Select(ce => ce.ParentType));
        //                entities = compositeEntities.Aggregate(entities, (current, compositeEntity) => PopulateCompositeEntityModel(compositeEntity, current, entitiesAndMetadata, verbose));
        //            }

        //            foreach (var entity in entities)
        //            {
        //                // we'll address composite entities separately
        //                if (compositeEntityTypes.Contains(entity.Type))
        //                {
        //                    continue;
        //                }

        //                AddProperty(entitiesAndMetadata, ExtractNormalizedEntityName(entity), ExtractEntityValue(entity));

        //                if (verbose)
        //                {
        //                    AddProperty((JObject)entitiesAndMetadata[_metadataKey], ExtractNormalizedEntityName(entity), ExtractEntityMetadata(entity));
        //                }
        //            }

        //            return entitiesAndMetadata;
        //        }

        //        private static JToken Number(dynamic value)
        //        {
        //            if (value == null)
        //            {
        //                return null;
        //            }

        //            return long.TryParse((string)value, out var longVal) ?
        //                            new JValue(longVal) :
        //                            new JValue(double.Parse((string)value));
        //        }

        //        private static JToken ExtractEntityValue(EntityModel entity)
        //        {
        //#pragma warning disable IDE0007 // Use implicit type
        //            if (entity.AdditionalProperties == null || !entity.AdditionalProperties.TryGetValue("resolution", out dynamic resolution))
        //#pragma warning restore IDE0007 // Use implicit type
        //            {
        //                return entity.Entity;
        //            }

        //            if (entity.Type.StartsWith("builtin.datetime."))
        //            {
        //                return JObject.FromObject(resolution);
        //            }
        //            else if (entity.Type.StartsWith("builtin.datetimeV2."))
        //            {
        //                if (resolution.values == null || resolution.values.Count == 0)
        //                {
        //                    return JArray.FromObject(resolution);
        //                }

        //                var resolutionValues = (IEnumerable<dynamic>)resolution.values;
        //                var type = resolution.values[0].type;
        //                var timexes = resolutionValues.Select(val => val.timex);
        //                var distinctTimexes = timexes.Distinct();
        //                return new JObject(new JProperty("type", type), new JProperty("timex", JArray.FromObject(distinctTimexes)));
        //            }
        //            else
        //            {
        //                switch (entity.Type)
        //                {
        //                    case "builtin.number":
        //                    case "builtin.ordinal": return Number(resolution.value);
        //                    case "builtin.percentage":
        //                        {
        //                            var svalue = (string)resolution.value;
        //                            if (svalue.EndsWith("%"))
        //                            {
        //                                svalue = svalue.Substring(0, svalue.Length - 1);
        //                            }

        //                            return Number(svalue);
        //                        }

        //                    case "builtin.age":
        //                    case "builtin.dimension":
        //                    case "builtin.currency":
        //                    case "builtin.temperature":
        //                        {
        //                            var units = (string)resolution.unit;
        //                            var val = Number(resolution.value);
        //                            var obj = new JObject();
        //                            if (val != null)
        //                            {
        //                                obj.Add("number", val);
        //                            }

        //                            obj.Add("units", units);
        //                            return obj;
        //                        }

        //                    default:
        //                        return resolution.value ?? JArray.FromObject(resolution.values);
        //                }
        //            }
        //        }

        //        private static JObject ExtractEntityMetadata(EntityModel entity)
        //        {
        //            dynamic obj = JObject.FromObject(new
        //            {
        //                startIndex = (int)entity.StartIndex,
        //                endIndex = (int)entity.EndIndex + 1,
        //                text = entity.Entity,
        //                type = entity.Type,
        //            });
        //            if (entity.AdditionalProperties != null)
        //            {
        //                if (entity.AdditionalProperties.TryGetValue("score", out var score))
        //                {
        //                    obj.score = (double)score;
        //                }

        //#pragma warning disable IDE0007 // Use implicit type
        //                if (entity.AdditionalProperties.TryGetValue("resolution", out dynamic resolution) && resolution.subtype != null)
        //#pragma warning restore IDE0007 // Use implicit type
        //                {
        //                    obj.subtype = resolution.subtype;
        //                }
        //            }

        //            return obj;
        //        }

        //        private static string ExtractNormalizedEntityName(EntityModel entity)
        //        {
        //            // Type::Role -> Role
        //            var type = entity.Type.Split(':').Last();
        //            if (type.StartsWith("builtin.datetimeV2."))
        //            {
        //                type = "datetime";
        //            }

        //            if (type.StartsWith("builtin.currency"))
        //            {
        //                type = "money";
        //            }

        //            if (type.StartsWith("builtin."))
        //            {
        //                type = type.Substring(8);
        //            }

        //            var role = entity.AdditionalProperties != null && entity.AdditionalProperties.ContainsKey("role") ? (string)entity.AdditionalProperties["role"] : string.Empty;
        //            if (!string.IsNullOrWhiteSpace(role))
        //            {
        //                type = role;
        //            }

        //            return type.Replace('.', '_').Replace(' ', '_');
        //        }

        //        private static IList<EntityModel> PopulateCompositeEntityModel(CompositeEntityModel compositeEntity, IList<EntityModel> entities, JObject entitiesAndMetadata, bool verbose)
        //        {
        //            var childrenEntites = new JObject();
        //            var childrenEntitiesMetadata = new JObject();
        //            if (verbose)
        //            {
        //                childrenEntites[_metadataKey] = new JObject();
        //            }

        //            // This is now implemented as O(n^2) search and can be reduced to O(2n) using a map as an optimization if n grows
        //            var compositeEntityMetadata = entities.FirstOrDefault(e => e.Type == compositeEntity.ParentType && e.Entity == compositeEntity.Value);

        //            // This is an error case and should not happen in theory
        //            if (compositeEntityMetadata == null)
        //            {
        //                return entities;
        //            }

        //            if (verbose)
        //            {
        //                childrenEntitiesMetadata = ExtractEntityMetadata(compositeEntityMetadata);
        //                childrenEntites[_metadataKey] = new JObject();
        //            }

        //            var coveredSet = new HashSet<EntityModel>();
        //            foreach (var child in compositeEntity.Children)
        //            {
        //                foreach (var entity in entities)
        //                {
        //                    // We already covered this entity
        //                    if (coveredSet.Contains(entity))
        //                    {
        //                        continue;
        //                    }

        //                    // This entity doesn't belong to this composite entity
        //                    if (child.Type != entity.Type || !CompositeContainsEntity(compositeEntityMetadata, entity))
        //                    {
        //                        continue;
        //                    }

        //                    // Add to the set to ensure that we don't consider the same child entity more than once per composite
        //                    coveredSet.Add(entity);
        //                    AddProperty(childrenEntites, ExtractNormalizedEntityName(entity), ExtractEntityValue(entity));

        //                    if (verbose)
        //                    {
        //                        AddProperty((JObject)childrenEntites[_metadataKey], ExtractNormalizedEntityName(entity), ExtractEntityMetadata(entity));
        //                    }
        //                }
        //            }

        //            AddProperty(entitiesAndMetadata, ExtractNormalizedEntityName(compositeEntityMetadata), childrenEntites);
        //            if (verbose)
        //            {
        //                AddProperty((JObject)entitiesAndMetadata[_metadataKey], ExtractNormalizedEntityName(compositeEntityMetadata), childrenEntitiesMetadata);
        //            }

        //            // filter entities that were covered by this composite entity
        //            return entities.Except(coveredSet).ToList();
        //        }

        //        private static bool CompositeContainsEntity(EntityModel compositeEntityMetadata, EntityModel entity)
        //            => entity.StartIndex >= compositeEntityMetadata.StartIndex &&
        //                   entity.EndIndex <= compositeEntityMetadata.EndIndex;

        //        /// <summary>
        //        /// If a property doesn't exist add it to a new array, otherwise append it to the existing array.
        //        /// </summary>
        //        private static void AddProperty(JObject obj, string key, JToken value)
        //        {
        //            if (((IDictionary<string, JToken>)obj).ContainsKey(key))
        //            {
        //                ((JArray)obj[key]).Add(value);
        //            }
        //            else
        //            {
        //                obj[key] = new JArray(value);
        //            }
        //        }

        //        private static void AddProperties(LuisResult luis, RecognizerResult result)
        //        {
        //            if (luis.SentimentAnalysis != null)
        //            {
        //                result.Properties.Add("sentiment", new JObject(
        //                    new JProperty("label", luis.SentimentAnalysis.Label),
        //                    new JProperty("score", luis.SentimentAnalysis.Score)));
        //            }
        //        }

        //        private async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        //        {
        //            BotAssert.ContextNotNull(turnContext);

        //            if (turnContext.Activity.Type != ActivityTypes.Message)
        //            {
        //                return null;
        //            }

        //            var utterance = turnContext.Activity?.AsMessageActivity()?.Text;

        //            if (string.IsNullOrWhiteSpace(utterance))
        //            {
        //                throw new ArgumentNullException(nameof(utterance));
        //            }

        //            var luisResult = await _runtime.Prediction.ResolveAsync(
        //                _application.ApplicationId,
        //                utterance,
        //                timezoneOffset: _options.TimezoneOffset,
        //                verbose: _options.IncludeAllIntents,
        //                staging: _options.Staging,
        //                spellCheck: _options.SpellCheck,
        //                bingSpellCheckSubscriptionKey: _options.BingSpellCheckSubscriptionKey,
        //                log: _options.Log ?? true,
        //                cancellationToken: cancellationToken).ConfigureAwait(false);

        //            var recognizerResult = new RecognizerResult
        //            {
        //                Text = utterance,
        //                AlteredText = luisResult.AlteredQuery,
        //                Intents = GetIntents(luisResult),
        //                Entities = ExtractEntitiesAndMetadata(luisResult.Entities, luisResult.CompositeEntities, _options.IncludeInstanceData ?? true),
        //            };
        //            AddProperties(luisResult, recognizerResult);
        //            if (_includeApiResults)
        //            {
        //                recognizerResult.Properties.Add("luisResult", luisResult);
        //            }

        //            var traceInfo = JObject.FromObject(
        //                new
        //                {
        //                    recognizerResult,
        //                    luisModel = new
        //                    {
        //                        ModelID = _application.ApplicationId,
        //                    },
        //                    luisOptions = _options,
        //                    luisResult,
        //                });

        //            await turnContext.TraceActivityAsync("LuisRecognizer", traceInfo, LuisTraceType, LuisTraceLabel, cancellationToken).ConfigureAwait(false);
        //            return recognizerResult;
        //        }
        public Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            throw new NotImplementedException();
        }
    }
}
