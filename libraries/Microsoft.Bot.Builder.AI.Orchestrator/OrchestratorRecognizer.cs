// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.BotFramework.Orchestrator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Orchestrator
{
    /// <summary>
    /// Class that represents an adaptive Orchestrator recognizer.
    /// </summary>
    public class OrchestratorRecognizer : Recognizer
    {
        /// <summary>
        /// The Kind name for this recognizer.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.OrchestratorRecognizer";

        /// <summary>
        /// Property key in RecognizerResult that holds the full recognition result from Orchestrator core.
        /// </summary>
        public const string ResultProperty = "result";

        /// <summary>
        /// Property key used when storing extracted entities in a custom event within telemetry.
        /// </summary>
        public const string EntitiesProperty = "entityResult";

        private const float UnknownIntentFilterScore = 0.4F;
        private static ConcurrentDictionary<string, OrchestratorDictionaryEntry> orchestratorMap = new ConcurrentDictionary<string, OrchestratorDictionaryEntry>();
        private OrchestratorDictionaryEntry _orchestrator = null;
        private ILabelResolver _resolver = null;
        private bool _isResolverMockup = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestratorRecognizer"/> class.
        /// </summary>
        [JsonConstructor]
        public OrchestratorRecognizer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestratorRecognizer"/> class.
        /// </summary>
        /// <param name="modelFolder">Specifies the base model folder.</param>
        /// <param name="snapshotFile">Specifies full path to the snapshot file.</param>
        /// <param name="resolverExternal">External label resolver object.</param>
        public OrchestratorRecognizer(string modelFolder, string snapshotFile, ILabelResolver resolverExternal = null)
        {
            InitializeModel(modelFolder, snapshotFile, resolverExternal);
        }

        /// <summary>
        /// Gets or sets the folder path to Orchestrator base model to use.
        /// </summary>
        /// <value>
        /// Model path.
        /// </value>
        [JsonProperty("modelFolder")]
        public string ModelFolder { get; set; }

        /// <summary>
        /// Gets or sets the full path to Orchestrator snapshot file to use.
        /// </summary>
        /// <value>
        /// Snapshot path.
        /// </value>
        [JsonProperty("snapshotFile")]
        public string SnapshotFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether personal information should be logged in telemetry.
        /// </summary>
        /// <value>
        /// The flag to indicate in personal information should be logged in telemetry.
        /// </value>
        [JsonProperty("logPersonalInformation")]
        public bool LogPersonalInformation { get; set; }

        /// <summary>
        /// Gets or sets an external entity recognizer.
        /// </summary>
        /// <remarks>This recognizer is run before calling Orchestrator and the entities are merged with Orchestrator results.</remarks>
        /// <value>Recognizer.</value>
        [JsonProperty("externalEntityRecognizer")]
        public Recognizer ExternalEntityRecognizer { get; set; }

        /// <summary>
        /// Gets or sets the disambiguation score threshold.
        /// </summary>
        /// <value>
        /// Recognizer returns ChooseIntent (disambiguation) if other intents are classified within this threshold of the top scoring intent.
        /// </value>
        [JsonProperty("disambiguationScoreThreshold")]
        public double DisambiguationScoreThreshold { get; set; } = 0.05F;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets detect ambiguous intents.
        /// </summary>
        /// <value>
        /// When true, recognizer will look for ambiguous intents - those within specified threshold to top scoring intent.
        /// </value>
        [JsonProperty("detectAmbiguousIntents")]
        public bool DetectAmbiguousIntents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable or disable entity-extraction logic.
        /// NOTE: SHOULD consider removing this flag in the next major SDK release (V5).
        /// </summary>
        /// <value>
        /// The flag for enabling or disabling entity-extraction function.
        /// </value>
        public bool ScoreEntities { get; set; } = true;

        /// <summary>
        /// Return recognition results.
        /// </summary>
        /// <param name="dc">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="activity">The incoming activity received from the user. The Text property value is used as the query text for QnA Maker.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <returns>A <see cref="RecognizerResult"/> containing the QnA Maker result.</returns>
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dc, Schema.Activity activity, CancellationToken cancellationToken, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            if (_resolver == null)
            {
                string modelFolder = ModelFolder;
                string snapshotFile = SnapshotFile;
                InitializeModel(modelFolder, snapshotFile, null);
            }

            var text = activity.Text ?? string.Empty;
            var detectAmbiguity = DetectAmbiguousIntents;

            var recognizerResult = new RecognizerResult()
            {
                Text = text,
                Intents = new Dictionary<string, IntentScore>(),
            };

            if (string.IsNullOrWhiteSpace(text))
            {
                // nothing to recognize, return empty recognizerResult
                return recognizerResult;
            }

            // Score with orchestrator
            var results = _resolver.Score(text)?.ToList();

            if ((results != null) && results.Any())
            {
                // Add full recognition result as a 'result' property
                recognizerResult.Properties.Add(ResultProperty, results);

                var topScore = results[0].Score;

                // if top scoring intent is less than threshold, return None
                if (topScore < UnknownIntentFilterScore)
                {
                    // remove existing None intents
                    for (int i = 0; i < results.Count; ++i)
                    {
                        if (results[i].Label.Name == NoneIntent)
                        {
                            results.RemoveAt(i--);
                        }
                    }

                    results.Insert(0, new Result() { Score = 1.0, Label = new Label() { Name = NoneIntent, Type = LabelType.Intent } });
                    foreach (var result in results)
                    {
                        recognizerResult.Intents.Add(result.Label.Name, new IntentScore()
                        {
                            Score = result.Score
                        });
                    }
                }
                else
                {
                    // add top score
                    foreach (var result in results)
                    {
                        recognizerResult.Intents.Add(result.Label.Name, new IntentScore()
                        {
                            Score = result.Score
                        });
                    }

                    // Disambiguate if configured
                    if (detectAmbiguity)
                    {
                        var thresholdScore = DisambiguationScoreThreshold;
                        var classifyingScore = Math.Round(topScore, 2) - Math.Round(thresholdScore, 2);
                        var ambiguousResults = results.Where(item => item.Score >= classifyingScore).ToList();

                        if (ambiguousResults.Count > 1)
                        {
                            // create a RecognizerResult for each ambiguous result.
                            var recognizerResults = ambiguousResults.Select(result => new RecognizerResult()
                            {
                                Text = text,
                                AlteredText = result.ClosestText,
                                Entities = recognizerResult.Entities,
                                Properties = recognizerResult.Properties,
                                Intents = new Dictionary<string, IntentScore>()
                                {
                                    { result.Label.Name, new IntentScore() { Score = result.Score } }
                                },
                            });

                            // replace RecognizerResult with ChooseIntent => Ambiguous recognizerResults as candidates.
                            recognizerResult = CreateChooseIntentResult(recognizerResults.ToDictionary(result => Guid.NewGuid().ToString(), result => result));
                        }
                    }
                }
            }
            else
            {
                // Return 'None' if no intent matched.
                recognizerResult.Intents.Add(NoneIntent, new IntentScore() { Score = 1.0 });
            }

            if (ExternalEntityRecognizer != null)
            {
                // Run external recognition
                var externalResults = await ExternalEntityRecognizer.RecognizeAsync(dc, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);
                recognizerResult.Entities = externalResults.Entities;
            }

            TryScoreEntities(text, recognizerResult);

            // Add full recognition result as a 'result' property
            await dc.Context.TraceActivityAsync($"{nameof(OrchestratorRecognizer)}Result", JObject.FromObject(recognizerResult), nameof(OrchestratorRecognizer), "Orchestrator Recognition", cancellationToken).ConfigureAwait(false);
            TrackRecognizerResult(dc, $"{nameof(OrchestratorRecognizer)}Result", FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties, dc), telemetryMetrics);

            return recognizerResult;
        }

        /// <summary>
        /// Uses the RecognizerResult to create a list of properties to be included when tracking the result in telemetry.
        /// </summary>
        /// <param name="recognizerResult">Recognizer Result.</param>
        /// <param name="telemetryProperties">A list of properties to append or override the properties created using the RecognizerResult.</param>
        /// <param name="dialogContext">Dialog Context.</param>
        /// <returns>A dictionary that can be included when calling the TrackEvent method on the TelemetryClient.</returns>
        protected override Dictionary<string, string> FillRecognizerResultTelemetryProperties(RecognizerResult recognizerResult, Dictionary<string, string> telemetryProperties, DialogContext dialogContext = null)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext), "DialogContext needed for state in AdaptiveRecognizer.FillRecognizerResultTelemetryProperties method.");
            }

            var orderedIntents = recognizerResult.Intents.Any() ? recognizerResult.Intents.OrderByDescending(key => key.Value.Score) : null;
            var properties = new Dictionary<string, string>
            {
                { "TopIntent", recognizerResult.Intents.Any() ? orderedIntents.First().Key : null },
                { "TopIntentScore", recognizerResult.Intents.Any() ? orderedIntents.First().Value?.Score?.ToString("N1", CultureInfo.InvariantCulture) : null },
                { "NextIntent", recognizerResult.Intents.Count > 1 ? orderedIntents.ElementAtOrDefault(1).Key : null },
                { "NextIntentScore", recognizerResult.Intents.Count > 1 ? orderedIntents.ElementAtOrDefault(1).Value?.Score?.ToString("N1", CultureInfo.InvariantCulture) : null },
                { "Intents", recognizerResult.Intents.Any() ? JsonConvert.SerializeObject(recognizerResult.Intents) : null },
                { "Entities", recognizerResult.Entities?.ToString() },
                { "AdditionalProperties", recognizerResult.Properties.Any() ? JsonConvert.SerializeObject(recognizerResult.Properties) : null },
            };

            var logPersonalInfo = LogPersonalInformation;

            if (logPersonalInfo && !string.IsNullOrEmpty(recognizerResult.Text))
            {
                properties.Add("Text", recognizerResult.Text);
                properties.Add("AlteredText", recognizerResult.AlteredText);
            }

            // Additional Properties can override "stock" properties.
            if (telemetryProperties != null)
            {
                return telemetryProperties.Concat(properties)
                    .GroupBy(kv => kv.Key)
                    .ToDictionary(g => g.Key, g => g.First().Value);
            }

            return properties;
        }

        private static JToken EntityResultToJObject(string text, Result result)
        {
            Span span = result.Label.Span;
            return new JObject(
                new JProperty("type", result.Label.Name),
                new JProperty("score", result.Score),
                new JProperty("text", text.Substring((int)span.Offset, (int)span.Length)),
                new JProperty("start", (int)span.Offset),
                new JProperty("end", (int)(span.Offset + span.Length)));
        }

        private static JToken EntityResultToInstanceJObject(string text, Result result)
        {
            Span span = result.Label.Span;
            dynamic instance = new JObject();
            instance.startIndex = (int)span.Offset;
            instance.endIndex = (int)(span.Offset + span.Length);
            instance.score = result.Score;
            instance.text = text.Substring((int)span.Offset, (int)span.Length);
            instance.type = result.Label.Name;
            return instance;
        }

        private void TryScoreEntities(string text, RecognizerResult recognizerResult)
        {
            // It's impossible to extract entities without a _resolver object.
            if (_resolver == null)
            {
                return;
            }

            // Entity extraction can be controlled by the ScoreEntities flag.
            // NOTE: SHOULD consider removing this flag in the next major SDK release (V5).
            if (!this.ScoreEntities)
            {
                return;
            }

            // The following check is necessary to ensure that the _resolver object
            // is capable of entity exttraction. However, this check can also block
            // a mock-up _resolver.
            if (!_isResolverMockup)
            {
                if ((_orchestrator == null) || (!_orchestrator.IsEntityExtractionCapable))
                {
                    return;
                }
            }

            // As this method is TryScoreEntities, so it's best effort only, there should
            // not be any exception thrown out of this method.
            try
            {
                var results = _resolver.Score(text, LabelType.Entity);

                if ((results != null) && results.Any())
                {
                    recognizerResult.Properties.Add(EntitiesProperty, results);

                    if (recognizerResult.Entities == null)
                    {
                        recognizerResult.Entities = new JObject();
                    }

                    var entitiesResult = recognizerResult.Entities;
                    foreach (var result in results)
                    {
                        // add value
                        JToken values;
                        if (!entitiesResult.TryGetValue(result.Label.Name, StringComparison.OrdinalIgnoreCase, out values))
                        {
                            values = new JArray();
                            entitiesResult[result.Label.Name] = values;
                        }

                        // values came from an external entity recognizer, which may not make it a JArray.
                        if (values.Type != JTokenType.Array)
                        {
                            values = new JArray();
                        }

                        ((JArray)values).Add(EntityResultToJObject(text, result));

                        // get/create $instance
                        JToken instanceRoot;
                        if (!recognizerResult.Entities.TryGetValue("$instance", StringComparison.OrdinalIgnoreCase, out instanceRoot))
                        {
                            instanceRoot = new JObject();
                            recognizerResult.Entities["$instance"] = instanceRoot;
                        }

                        // instanceRoot came from an external entity recognizer, which may not make it a JObject.
                        if (instanceRoot.Type != JTokenType.Object)
                        {
                            instanceRoot = new JObject();
                        }

                        // add instanceData
                        JToken instanceData;
                        if (!((JObject)instanceRoot).TryGetValue(result.Label.Name, StringComparison.OrdinalIgnoreCase, out instanceData))
                        {
                            instanceData = new JArray();
                            instanceRoot[result.Label.Name] = instanceData;
                        }

                        // instanceData came from an external entity recognizer, which may not make it a JArray.
                        if (instanceData.Type != JTokenType.Array)
                        {
                            instanceData = new JArray();
                        }

                        ((JArray)instanceData).Add(EntityResultToInstanceJObject(text, result));
                    }
                }
            }
            catch (ApplicationException)
            {
                return; // ---- This is a "Try" function, i.e., best effort only, no exception.
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void InitializeModel(string modelFolder, string snapshotFile, ILabelResolver resolverExternal = null)
        {
            if (resolverExternal != null)
            {
                _resolver = resolverExternal;
                _isResolverMockup = true;
                return;
            }

            {
                if (string.IsNullOrWhiteSpace(modelFolder))
                {
                    throw new ArgumentNullException(nameof(modelFolder));
                }

                if (string.IsNullOrWhiteSpace(snapshotFile))
                {
                    throw new ArgumentNullException(nameof(snapshotFile));
                }
            }

            var fullModelFolder = Path.GetFullPath(PathUtils.NormalizePath(modelFolder));

            _orchestrator = orchestratorMap.GetOrAdd(fullModelFolder, path =>
            {
                // Create Orchestrator
                string entityModelFolder = null;
                bool isEntityExtractionCapable = false;
                try
                {
                    entityModelFolder = Path.Combine(path, "entity");
                    isEntityExtractionCapable = Directory.Exists(entityModelFolder);

                    return new OrchestratorDictionaryEntry()
                    {
                        Orchestrator = isEntityExtractionCapable ?
                            new BotFramework.Orchestrator.Orchestrator(path, entityModelFolder) :
                            new BotFramework.Orchestrator.Orchestrator(path),
                        IsEntityExtractionCapable = isEntityExtractionCapable
                    };
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        isEntityExtractionCapable ? $"Failed to find or load Model with path {path}, entity model path {entityModelFolder}" : $"Failed to find or load Model with path {path}",
                        ex);
                }
            });

            var fullSnapShotFile = Path.GetFullPath(PathUtils.NormalizePath(snapshotFile));

            // Load the snapshot
            byte[] snapShotByteArray = File.ReadAllBytes(fullSnapShotFile);

            // Create label resolver
            _resolver = this._orchestrator.Orchestrator.CreateLabelResolver(snapShotByteArray);
        }

        /// <summary>
        /// OrchestratorDictionaryEntry is used for the static orchestratorMap object.
        /// </summary>
        private class OrchestratorDictionaryEntry
        {
            /// <summary>
            /// Gets or sets the Orchestrator object.
            /// </summary>
            /// <value>
            /// The Orchestrator object.
            /// </value>
            public BotFramework.Orchestrator.Orchestrator Orchestrator
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the Orchestrator object is capable of entity extraction.
            /// </summary>
            /// <value>
            /// The IsEntityExtractionCapable flag.
            /// </value>
            public bool IsEntityExtractionCapable
            {
                get;
                set;
            }
        }
    }
}
