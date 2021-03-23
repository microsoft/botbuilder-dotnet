// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.BotFramework.Orchestrator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Orchestrator
{
    /// <summary>
    /// Class that represents an adaptive Orchestrator recognizer.
    /// </summary>
    public class OrchestratorAdaptiveRecognizer : AdaptiveRecognizer
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
        public const string EntitiesProperty = "entities";
        private const float UnknownIntentFilterScore = 0.4F;
        private static ConcurrentDictionary<string, BotFramework.Orchestrator.Orchestrator> orchestratorMap = new ConcurrentDictionary<string, BotFramework.Orchestrator.Orchestrator>();
        private string _modelFolder;
        private string _snapshotFile;
        private ILabelResolver _resolver = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestratorAdaptiveRecognizer"/> class.
        /// </summary>
        /// <param name="callerLine">Caller line.</param>
        /// <param name="callerPath">Caller path.</param>
        [JsonConstructor]
        public OrchestratorAdaptiveRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestratorAdaptiveRecognizer"/> class.
        /// </summary>
        /// <param name="modelFolder">Specifies the base model folder.</param>
        /// <param name="snapshotFile">Specifies full path to the snapshot file.</param>
        /// <param name="resolver">Label resolver.</param>
        public OrchestratorAdaptiveRecognizer(string modelFolder, string snapshotFile, ILabelResolver resolver = null)
        {
            _resolver = resolver;
            if (modelFolder == null)
            {
                throw new ArgumentNullException(nameof(modelFolder));
            }

            if (snapshotFile == null)
            {
                throw new ArgumentNullException(nameof(snapshotFile));
            }

            _modelFolder = modelFolder;
            _snapshotFile = snapshotFile;
            InitializeModel();
        }

        [JsonIgnore]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool ScoreEntities { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Gets or sets the folder path to Orchestrator base model to use.
        /// </summary>
        /// <value>
        /// Model path.
        /// </value>
        [JsonProperty("modelFolder")]
        public StringExpression ModelFolder { get; set; } = "=settings.orchestrator.modelFolder";

        /// <summary>
        /// Gets or sets the full path to Orchestrator snapshot file to use.
        /// </summary>
        /// <value>
        /// Snapshot path.
        /// </value>
        [JsonProperty("snapshotFile")]
        public StringExpression SnapshotFile { get; set; } = "=settings.orchestrator.snapshotFile";

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
        public NumberExpression DisambiguationScoreThreshold { get; set; } = 0.05F;

        /// <summary>
        /// Gets or sets detect ambiguous intents.
        /// </summary>
        /// <value>
        /// When true, recognizer will look for ambiguous intents - those within specified threshold to top scoring intent.
        /// </value>
        [JsonProperty("detectAmbiguousIntents")]
        public BoolExpression DetectAmbiguousIntents { get; set; } = false;

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
            var text = activity.Text ?? string.Empty;
            var detectAmbiguity = DetectAmbiguousIntents.GetValue(dc.State);

            _modelFolder = ModelFolder.GetValue(dc.State);
            _snapshotFile = SnapshotFile.GetValue(dc.State);

            InitializeModel();

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
            var results = _resolver.Score(text);

            // Add full recognition result as a 'result' property
            recognizerResult.Properties.Add(ResultProperty, results);

            if (results.Any())
            {
                var topScore = results[0].Score;

                // if top scoring intent is less than threshold, return None
                if (topScore < UnknownIntentFilterScore)
                {
                    recognizerResult.Intents.Add(NoneIntent, new IntentScore() { Score = 1.0 });
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
                        var thresholdScore = DisambiguationScoreThreshold.GetValue(dc.State);
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
            
            await dc.Context.TraceActivityAsync($"{nameof(OrchestratorAdaptiveRecognizer)}Result", JObject.FromObject(recognizerResult), nameof(OrchestratorAdaptiveRecognizer), "Orchestrator Recognition", cancellationToken).ConfigureAwait(false);
            TrackRecognizerResult(dc, $"{nameof(OrchestratorAdaptiveRecognizer)}Result", FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties, dc), telemetryMetrics);

            if (ExternalEntityRecognizer != null)
            {
                // Run external recognition
                var externalResults = await ExternalEntityRecognizer.RecognizeAsync(dc, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);
                recognizerResult.Entities = externalResults.Entities;
            }

            TryScoreEntities(text, recognizerResult);
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

            var (logPersonalInfo, error) = LogPersonalInformation.TryGetValue(dialogContext.State);

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
            if (!this.ScoreEntities)
            {
                return;
            }

            var results = _resolver.Score(text, LabelType.Entity);
            recognizerResult.Properties.Add(EntitiesProperty, results);

            if (results.Any())
            {
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

                    ((JArray)values).Add(EntityResultToJObject(text, result));

                    // get/create $instance
                    JToken instanceRoot;
                    if (!recognizerResult.Entities.TryGetValue("$instance", StringComparison.OrdinalIgnoreCase, out instanceRoot))
                    {
                        instanceRoot = new JObject();
                        recognizerResult.Entities["$instance"] = instanceRoot;
                    }

                    // add instanceData
                    JToken instanceData;
                    if (!((JObject)instanceRoot).TryGetValue(result.Label.Name, StringComparison.OrdinalIgnoreCase, out instanceData))
                    {
                        instanceData = new JArray();
                        instanceRoot[result.Label.Name] = instanceData;
                    }

                    ((JArray)instanceData).Add(EntityResultToInstanceJObject(text, result));
                }
            }
        }

        private void InitializeModel()
        {
            if (_modelFolder == null)
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentNullException("ModelFolder");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            if (_snapshotFile == null)
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentNullException("SnapshotFile");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            if (_resolver != null)
            {
                return;
            }

            var fullModelFolder = Path.GetFullPath(PathUtils.NormalizePath(_modelFolder));

            var orchestrator = orchestratorMap.GetOrAdd(fullModelFolder, path =>
            {
                // Create Orchestrator
                try
                {
                    string entityModelFolder = Path.Combine(path, "entity");
                    ScoreEntities = Directory.Exists(entityModelFolder);

                    return ScoreEntities ?
                        new BotFramework.Orchestrator.Orchestrator(path, entityModelFolder) :
                        new BotFramework.Orchestrator.Orchestrator(path);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to find or load Model", ex);
                }
            });

            var fullSnapShotFile = Path.GetFullPath(PathUtils.NormalizePath(_snapshotFile));

            // Load the snapshot
            string content = File.ReadAllText(fullSnapShotFile);
            byte[] snapShotByteArray = Encoding.UTF8.GetBytes(content);

            // Create label resolver
            _resolver = orchestrator.CreateLabelResolver(snapShotByteArray);
        }
    }
}
