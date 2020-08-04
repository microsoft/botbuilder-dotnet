// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Bot.Schema;
using Microsoft.Orchestrator;
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
        /// Intent name that will be produced by this recognizer if the child recognizers do not have consensus for intents.
        /// </summary>
        private const string ChooseIntent = "ChooseIntent";

        /// <summary>
        /// Property name for candidate intents that meet the ambiguity threshold.
        /// </summary>
        private const string CandidatesCollection = "candidates";

        private const float UnknownIntentFilterScore = 0.4F;
        private const string NoneIntent = "None";
        private static Microsoft.Orchestrator.Orchestrator orchestrator = null;
        private string _modelPath;
        private string _snapshotPath;
        private ILabelResolver _resolver = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestratorRecognizer"/> class.
        /// </summary>
        /// <param name="callerLine">caller line.</param>
        /// <param name="callerPath">caller path.</param>
        [JsonConstructor]
        public OrchestratorRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestratorRecognizer"/> class.
        /// </summary>
        /// <param name="modelPath">Path to NLR model.</param>
        /// <param name="snapshotPath">Path to snapshot.</param>
        public OrchestratorRecognizer(string modelPath, string snapshotPath)
        {
            if (modelPath == null)
            {
                throw new ArgumentNullException($"Missing `ModelPath` information.");
            }

            if (snapshotPath == null)
            {
                throw new ArgumentNullException($"Missing `SnapshotPath` information.");
            }

            _modelPath = modelPath;
            _snapshotPath = snapshotPath;
            InitializeModel();
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
            var detectAmbiguity = DetectAmbiguousIntents.GetValue(dialogContext.State);

            _modelPath = ModelPath.GetValue(dialogContext.State);
            _snapshotPath = SnapshotPath.GetValue(dialogContext.State);

            InitializeModel();

            var tempContext = new TurnContext(dialogContext.Context.Adapter, activity);
            foreach (var keyValue in dialogContext.Context.TurnState)
            {
                tempContext.TurnState[keyValue.Key] = keyValue.Value;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var recognizerResult = this.Recognize(tempContext);
            sw.Stop();
            Trace.TraceInformation($"Orchestrator recognize in {sw.ElapsedMilliseconds}ms");

            tempContext.Dispose();

            if (EntityRecognizers.Count != 0)
            {
                // Run entity recognition
                recognizerResult = await RecognizeEntitiesAsync(dialogContext, activity, recognizerResult).ConfigureAwait(false);
            }

            // Score with orchestrator
            recognizerResult.Properties.TryGetValue(ResultProperty, out var resultObject);

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

            await dialogContext.Context.TraceActivityAsync(nameof(OrchestratorRecognizer), JObject.FromObject(recognizerResult), nameof(OrchestratorRecognizer), "Orchestrator Recognition ", cancellationToken).ConfigureAwait(false);

            TrackRecognizerResult(dialogContext, nameof(OrchestratorRecognizer), FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties), telemetryMetrics);

            return recognizerResult;
        }

        /// <summary>
        /// Returns recognition result.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <returns>Recognizer rsult.</returns>
        public RecognizerResult Recognize(ITurnContext turnContext)
        {
            var text = turnContext.Activity.Text ?? string.Empty;
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
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var result = this._resolver.Score(text);
            sw.Stop();
            Console.WriteLine($"Orchestrator recognize : {sw.ElapsedMilliseconds}");

            if (result.Any())
            {
                AddTopScoringIntent(result, ref recognizerResult);
            }

            // Add full recognition result as a 'result' property
            recognizerResult.Properties.Add(ResultProperty, result);

            // Return 'None' if no intent matched.
            if (!recognizerResult.Intents.Any())
            {
                recognizerResult.Intents.Add(NoneIntent, new IntentScore() { Score = 1.0 });
            }

            return recognizerResult;
        }

        private static RecognizerResult AddTopScoringIntent(IReadOnlyList<Result> result, ref RecognizerResult recognizerResult)
        {
            var topScoringIntent = result[0].Label.Name;
            var topScore = result[0].Score;

            // if top scoring intent is less than threshold, return None
            if (topScore < UnknownIntentFilterScore)
            {
                recognizerResult.Intents.Add(NoneIntent, new IntentScore() { Score = 1.0 });
            }
            else
            {
                if (!recognizerResult.Intents.ContainsKey(topScoringIntent))
                {
                    recognizerResult.Intents.Add(topScoringIntent, new IntentScore()
                    {
                        Score = result[0].Score
                    });
                }
            }

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
            if (_modelPath == null)
            {
                throw new ArgumentNullException($"Missing `ModelPath` information.");
            }

            if (_snapshotPath == null)
            {
                throw new ArgumentNullException($"Missing `ShapshotPath` information.");
            }

            if (orchestrator == null)
            {
                var fullModelPath = Path.GetFullPath(PathUtils.NormalizePath(_modelPath));
                Stopwatch sw = new Stopwatch();

                // Create Orchestrator 
                try
                {
                    sw.Start();
                    orchestrator = new Microsoft.Orchestrator.Orchestrator(fullModelPath);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    throw new Exception("Failed to find or load Model", ex);
                }

                sw.Stop();
                Console.WriteLine($"Model load time:{sw.ElapsedMilliseconds}");
            }

            if (_resolver == null)
            {
                var fullSnapShotPath = Path.GetFullPath(PathUtils.NormalizePath(_snapshotPath));

                // Load the snapshot
                string content = File.ReadAllText(fullSnapShotPath);
                byte[] snapShotByteArray = Encoding.UTF8.GetBytes(content);

                // Create label resolver
                _resolver = orchestrator.CreateLabelResolver(snapShotByteArray);
            }
        }
    }
}
