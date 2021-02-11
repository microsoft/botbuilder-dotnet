// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizer implementation, intended to be a base class for adaptive recognizers.
    /// </summary>
    public abstract class AdaptiveRecognizer : Recognizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveRecognizer"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        protected AdaptiveRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Gets or sets the flag to determine if telemetry should include personal information in its log.
        /// </summary>
        /// <value>
        /// The flag to indicate if telemetry should log personal information.
        /// </value>
        [JsonProperty("logPersonalInformation")]
        public BoolExpression LogPersonalInformation { get; set; } = "=settings.telemetry.logPersonalInformation";

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

            var properties = new Dictionary<string, string>
            {
                { "TopIntent", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Key : null },
                { "TopIntentScore", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Value?.Score?.ToString("N1", CultureInfo.InvariantCulture) : null },
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
    }
}
