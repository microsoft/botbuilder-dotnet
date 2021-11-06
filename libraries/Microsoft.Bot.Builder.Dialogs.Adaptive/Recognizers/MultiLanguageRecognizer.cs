﻿// Licensed under the MIT License.
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
    public class MultiLanguageRecognizer : Recognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.MultiLanguageRecognizer";

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
        public LanguagePolicy LanguagePolicy { get; set; } = new LanguagePolicy();
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

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var policy = new List<string>();
            if (activity.Locale != null && LanguagePolicy.TryGetValue(activity.Locale, out string[] targetpolicy))
            {
                policy.AddRange(targetpolicy);
            }

            if (LanguagePolicy.TryGetValue(string.Empty, out string[] defaultPolicy))
            {
                // we now explictly add defaultPolicy instead of coding that into target's policy
                policy.AddRange(defaultPolicy);
            }

            foreach (var option in policy)
            {
                if (this.Recognizers.TryGetValue(option, out var recognizer))
                {
                    var result = await recognizer.RecognizeAsync(dialogContext, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);
                    this.TrackRecognizerResult(dialogContext, "MultiLanguagesRecognizerResult", this.FillRecognizerResultTelemetryProperties(result, telemetryProperties), telemetryMetrics);
                    return result;
                }
            }

            this.TrackRecognizerResult(dialogContext, "MultiLanguagesRecognizerResult", this.FillRecognizerResultTelemetryProperties(new RecognizerResult() { }, telemetryProperties), telemetryMetrics);
            
            // nothing recognized
            return new RecognizerResult() { };
        }
    }
}
