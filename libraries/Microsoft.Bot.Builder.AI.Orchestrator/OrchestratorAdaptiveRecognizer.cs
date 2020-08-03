// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Orchestrator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Orchestrator
{
    public class OrchestratorAdaptiveRecognizer : Recognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.OrchestratorRecognizer";

        /// <summary>
        /// Intent name that will be produced by this recognizer if the child recognizers do not have consensus for intents.
        /// </summary>
        private const string ChooseIntent = "ChooseIntent";

        /// <summary>
        /// Property name for candidate intents that meet the ambiguity threshold.
        /// </summary>
        private const string CandidatesCollection = "candidates";

        private static string modelPath = null;
        private string snapshotPath = null;
        private OrchestratorRecognizer recognizer = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestratorAdaptiveRecognizer"/> class.
        /// </summary>
        /// <param name="callerLine">caller line.</param>
        /// <param name="callerPath">caller path.</param>
        [JsonConstructor]
        public OrchestratorAdaptiveRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Gets or sets the full path to the NLR model to use.
        /// </summary>
        /// <value>
        /// Model path.
        /// </value>
        [JsonProperty("modelPath")]
        public StringExpression ModelPath { get; set; }

        /// <summary>
        /// Gets or sets the full path to the snapshot to use.
        /// </summary>
        /// <value>
        /// Snapshot path.
        /// </value>
        [JsonProperty("snapshotPath")]
        public StringExpression SnapshotPath { get; set; }

        /// <summary>
        /// Gets or sets the entity recognizers.
        /// </summary>
        /// <value>
        /// The entity recognizers.
        /// </value>
        [JsonProperty("entityRecognizers")]
#pragma warning disable CA2227 // Collection properties should be read only (keeping this consistent with RegexRecognizer)
        public List<EntityRecognizer> EntityRecognizers { get; set; } = new List<EntityRecognizer>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the disambiguation score threshold.
        /// </summary>
        /// <value>
        /// Recognizer returns ChooseIntent (disambiguation) if other intents are classified within this score of the top scoring intent.
        /// </value>
        [JsonProperty("disambiguationScoreThreshold")]
        public NumberExpression DisambiguationScoreThreshold { get; set; } = 0.05F;

        /// <summary>
        /// Gets or sets detect ambiguous intents.
        /// </summary>
        /// <value>
        /// When true, recognizer will look for ambiguous intents (intents with close recognition scores from top scoring intent).
        /// </value>
        [JsonProperty("detectAmbiguousIntents")]
        public BoolExpression DetectAmbiguousIntents { get; set; } = false;

        /// <summary>
        /// Return recognition results.
        /// </summary>
        /// <param name="dialogContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="activity">The incoming activity received from the user. The Text property value is used as the query text for QnA Maker.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <returns>A <see cref="RecognizerResult"/> containing the QnA Maker result.</returns>
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Schema.Activity activity, CancellationToken cancellationToken, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var text = activity.Text ?? string.Empty;
            var detectAmbiguity = DetectAmbiguousIntents.GetValue(dialogContext.State);

            modelPath = ModelPath.GetValue(dialogContext.State);
            snapshotPath = SnapshotPath.GetValue(dialogContext.State);

            InitializeModel();

            var tempContext = new TurnContext(dialogContext.Context.Adapter, activity);
            foreach (var keyValue in dialogContext.Context.TurnState)
            {
                tempContext.TurnState[keyValue.Key] = keyValue.Value;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var recognizerResult = await recognizer.RecognizeAsync(tempContext, cancellationToken).ConfigureAwait(false);
            sw.Stop();
            Trace.TraceInformation($"Orchestrator recognize in {sw.ElapsedMilliseconds}ms");

            tempContext.Dispose();

            if (EntityRecognizers.Count != 0)
            {
                // Run entity recognition
                recognizerResult = await RecognizeEntitiesAsync(dialogContext, activity, recognizerResult).ConfigureAwait(false);
            }

            // Score with orchestrator
            recognizerResult.Properties.TryGetValue(OrchestratorRecognizer.ResultProperty, out var resultObject);

            var result = (IReadOnlyCollection<Result>)resultObject;

            if (result.Any())
            {
                // Disambiguate if configured
                if (detectAmbiguity == true)
                {
                    var topScore = result.First().Score;
                    var thresholdScore = DisambiguationScoreThreshold.GetValue(dialogContext.State);
                    var classifyingScore = Math.Round(topScore, 2) - Math.Round(thresholdScore, 2);
                    var ambiguousIntents = result.Where(item => item.Score >= classifyingScore);

                    if (ambiguousIntents.Any())
                    {
                        // Add ambiguous intents that meet the threshold as candidates.
                        var candidates = new JObject();
                        foreach (Result ambiguousIntent in ambiguousIntents)
                        {
                            var candidate = new JObject();
                            candidate.Add("intent", ambiguousIntent.Label.Name);
                            candidate.Add("score", ambiguousIntent.Score);
                            candidate.Add("closestText", ambiguousIntent.ClosestText);
                            var recoResult = new RecognizerResult();
                            recoResult.Intents.Add(ambiguousIntent.Label.Name, new IntentScore()
                            {
                                Score = ambiguousIntent.Score
                            });
                            recoResult.Entities = recognizerResult.Entities;
                            recoResult.Text = recognizerResult.Text;
                            candidate.Add("result", JObject.FromObject(recoResult));
                            candidates.Add(ambiguousIntent.Label.Name, candidate);
                        }

                        recognizerResult.Intents.Add(ChooseIntent, new IntentScore() { Score = 1.0 });
                        recognizerResult.Properties = new Dictionary<string, object>() { { CandidatesCollection, candidates } };
                    }
                }
            }

            await dialogContext.Context.TraceActivityAsync(nameof(OrchestratorAdaptiveRecognizer), JObject.FromObject(recognizerResult), nameof(OrchestratorAdaptiveRecognizer), "Orchestrator Recognition ", cancellationToken).ConfigureAwait(false);

            TrackRecognizerResult(dialogContext, nameof(OrchestratorAdaptiveRecognizer), FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties), telemetryMetrics);

            return recognizerResult;
        }

        private async Task<RecognizerResult> RecognizeEntitiesAsync(DialogContext dialogContext, Schema.Activity activity, RecognizerResult recognizerResult)
        {
            var text = activity.Text ?? string.Empty;
            var entityPool = new List<Entity>();
            if (EntityRecognizers != null)
            {
                // add entities from regexrecgonizer to the entities pool
                var textEntity = new TextEntity(text);
                textEntity.Properties["start"] = 0;
                textEntity.Properties["end"] = text.Length;
                textEntity.Properties["score"] = 1.0;

                entityPool.Add(textEntity);

                // process entities using EntityRecognizerSet
                var entitySet = new EntityRecognizerSet(EntityRecognizers);
                var newEntities = await entitySet.RecognizeEntitiesAsync(dialogContext, activity, entityPool).ConfigureAwait(false);
                if (newEntities.Any())
                {
                    entityPool.AddRange(newEntities);
                }

                entityPool.Remove(textEntity);
            }

            // map entityPool of Entity objects => RecognizerResult entity format
            recognizerResult.Entities = new JObject();

            foreach (var entityResult in entityPool)
            {
                // add value
                JToken values;
                if (!recognizerResult.Entities.TryGetValue(entityResult.Type, StringComparison.OrdinalIgnoreCase, out values))
                {
                    values = new JArray();
                    recognizerResult.Entities[entityResult.Type] = values;
                }

                // The Entity type names are not consistent, map everything to camelcase so we can process them cleaner.
                var entity = JObject.FromObject(entityResult);
                ((JArray)values).Add(entity.GetValue("text", StringComparison.InvariantCulture));

                // get/create $instance
                JToken instanceRoot;
                if (!recognizerResult.Entities.TryGetValue("$instance", StringComparison.OrdinalIgnoreCase, out instanceRoot))
                {
                    instanceRoot = new JObject();
                    recognizerResult.Entities["$instance"] = instanceRoot;
                }

                // add instanceData
                JToken instanceData;
                if (!((JObject)instanceRoot).TryGetValue(entityResult.Type, StringComparison.OrdinalIgnoreCase, out instanceData))
                {
                    instanceData = new JArray();
                    instanceRoot[entityResult.Type] = instanceData;
                }

                JObject instance = new JObject();
                instance.Add("startIndex", entity.GetValue("start", StringComparison.InvariantCulture));
                instance.Add("endIndex", entity.GetValue("end", StringComparison.InvariantCulture));
                instance.Add("score", (double)1.0);
                instance.Add("text", entity.GetValue("text", StringComparison.InvariantCulture));
                instance.Add("type", entity.GetValue("type", StringComparison.InvariantCulture));
                instance.Add("resolution", entity.GetValue("resolution", StringComparison.InvariantCulture));
                ((JArray)instanceData).Add(instance);
            }

            return recognizerResult;
        }

        private void InitializeModel()
        {
            if (modelPath == null)
            {
                throw new ArgumentNullException($"Missing `ModelPath` information.");
            }

            if (snapshotPath == null)
            {
                throw new ArgumentNullException($"Missing `ShapshotPath` information.");
            }

            if (recognizer == null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                recognizer = new OrchestratorRecognizer(modelPath, snapshotPath);
                sw.Stop();
                Trace.TraceInformation($"Orchestrator loaded in {sw.ElapsedMilliseconds}ms");
            }
        }
    }
}
