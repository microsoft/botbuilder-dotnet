// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Orchestrator
{
    /// <summary>
    /// Class that represents an adaptive Orchestrator recognizer.
    /// </summary>
    public class OrchestratorRecognizer : IRecognizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestratorRecognizer"/> class.
        /// </summary>
        [JsonConstructor]
        public OrchestratorRecognizer()
        {
        }

        /// <summary>
        /// Gets or sets the id for the recognizer.
        /// </summary>
        /// <value>
        /// The id for the recognizer.  Useful for looking up specific recognizer in multiple recognizers scenario.
        /// </value>
        [JsonProperty("id")]
        public string Id { get; set; }

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
        public float DisambiguationScoreThreshold { get; set; } = 0.05F;

        /// <summary>
        /// Gets or sets a value indicating whether detect ambiguous intents.
        /// </summary>
        /// <value>
        /// When true, recognizer will look for ambiguous intents - those within specified threshold to top scoring intent.
        /// </value>
        [JsonProperty("detectAmbiguousIntents")]
        public bool DetectAmbiguousIntents { get; set; } = false;

        /// <inheritdoc/>
        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var rec = new OrchestratorAdaptiveRecognizer
            {
                Id = this.Id,
                DetectAmbiguousIntents = this.DetectAmbiguousIntents,
                ModelFolder = this.ModelFolder,
                SnapshotFile = this.SnapshotFile,
                DisambiguationScoreThreshold = this.DisambiguationScoreThreshold,
                ExternalEntityRecognizer = this.ExternalEntityRecognizer,
            };

            var dc = new DialogContext(new DialogSet(), turnContext, new DialogState());
            return await rec.RecognizeAsync(dc, turnContext.Activity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            var result = await RecognizeAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return ObjectPath.MapValueTo<T>(result);
        }
    }
}
