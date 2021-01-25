// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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
using Microsoft.BotFramework.Orchestrator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Orchestrator
{
    /// <summary>
    /// Class that represents an adaptive Orchestrator recognizer.
    /// </summary>
    public class OrchestratorAdaptiveRecognizer : Recognizer
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

        private const float UnknownIntentFilterScore = 0.4F;
        private static BotFramework.Orchestrator.Orchestrator orchestrator = null;
        private string _modelPath;
        private string _snapshotPath;
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
        /// <param name="modelPath">Path to NLR model.</param>
        /// <param name="snapshotPath">Path to snapshot.</param>
        /// <param name="resolver">Label resolver.</param>
        public OrchestratorAdaptiveRecognizer(string modelPath, string snapshotPath, ILabelResolver resolver = null)
        {
            _resolver = resolver;
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
        /// Gets or sets the folder path to Orchestrator base model to use.
        /// </summary>
        /// <value>
        /// Model path.
        /// </value>
        [JsonProperty("modelPath")]
        public StringExpression ModelPath { get; set; } = "=settings.orchestrator.modelPath";

        /// <summary>
        /// Gets or sets the full path to Orchestrator snapshot file to use.
        /// </summary>
        /// <value>
        /// Snapshot path.
        /// </value>
        [JsonProperty("snapshotPath")]
        public StringExpression SnapshotPath { get; set; } = "=settings.orchestrator.snapshotPath";

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

            _modelPath = ModelPath.GetValue(dc.State);
            _snapshotPath = SnapshotPath.GetValue(dc.State);

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

            if (ExternalEntityRecognizer != null)
            {
                // Run external recognition
                var externalResults = await ExternalEntityRecognizer.RecognizeAsync(dc, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);
                recognizerResult.Entities = externalResults.Entities;
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
                    recognizerResult.Intents.Add(results[0].Label.Name, new IntentScore()
                    {
                        Score = results[0].Score
                    });

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

            await dc.Context.TraceActivityAsync(nameof(OrchestratorAdaptiveRecognizer), JObject.FromObject(recognizerResult), nameof(OrchestratorAdaptiveRecognizer), "Orchestrator Recognition ", cancellationToken).ConfigureAwait(false);
            TrackRecognizerResult(dc, nameof(OrchestratorAdaptiveRecognizer), FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties), telemetryMetrics);

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

            if (orchestrator == null && _resolver == null)
            {
                var fullModelPath = Path.GetFullPath(PathUtils.NormalizePath(_modelPath));

                // Create Orchestrator
                try
                {
                    orchestrator = new BotFramework.Orchestrator.Orchestrator(fullModelPath);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to find or load Model", ex);
                }
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
