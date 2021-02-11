// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Defines map of languages -> recognizer.
    /// </summary>
    public class MultiLanguageRecognizer : AdaptiveRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.MultiLanguageRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLanguageRecognizer"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public MultiLanguageRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Gets or sets policy for languages fallback. 
        /// </summary>
        /// <value>
        /// Policy for languages fallback. 
        /// </value>
        [JsonProperty("languagePolicy")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public LanguagePolicy LanguagePolicy { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets map of languages -> IRecognizer.
        /// </summary>
        /// <value>
        /// Map of languages -> IRecognizer.
        /// </value>
        [JsonProperty("recognizers")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public IDictionary<string, Recognizer> Recognizers { get; set; } = new Dictionary<string, Recognizer>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Runs current DialogContext.TurnContext.Activity through a recognizer and returns a <see cref="RecognizerResult"/>.
        /// </summary>
        /// <param name="dialogContext">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="activity"><see cref="Activity"/> to recognize.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/> of the task.</param>
        /// <param name="telemetryProperties">Optional, additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Optional, additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <returns>Analysis of utterance.</returns>
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var languagePolicy = LanguagePolicy ??
                    dialogContext.Services.Get<LanguagePolicy>() ??
                    new LanguagePolicy();

            var policy = new List<string>();
            if (activity.Locale != null && languagePolicy.TryGetValue(activity.Locale, out string[] targetpolicy))
            {
                policy.AddRange(targetpolicy);
            }

            if (languagePolicy.TryGetValue(string.Empty, out string[] defaultPolicy))
            {
                // we now explictly add defaultPolicy instead of coding that into target's policy
                policy.AddRange(defaultPolicy);
            }

            foreach (var option in policy)
            {
                if (Recognizers.TryGetValue(option, out var recognizer))
                {
                    var result = await recognizer.RecognizeAsync(dialogContext, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);
                    TrackRecognizerResult(dialogContext, "MultiLanguageRecognizerResult", FillRecognizerResultTelemetryProperties(result, telemetryProperties, dialogContext), telemetryMetrics);
                    return result;
                }
            }

            TrackRecognizerResult(dialogContext, "MultiLanguageRecognizerResult", FillRecognizerResultTelemetryProperties(new RecognizerResult() { }, telemetryProperties, dialogContext), telemetryMetrics);
            
            // nothing recognized
            return new RecognizerResult() { };
        }
    }
}
