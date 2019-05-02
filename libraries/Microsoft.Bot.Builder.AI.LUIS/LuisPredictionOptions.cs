// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Optional parameters for a LUIS prediction request.
    /// </summary>
    public class LuisPredictionOptions
    {
        /// <summary>
        /// Gets or sets the Bing Spell Check subscription key.
        /// </summary>
        /// <value>
        /// The Bing Spell Check subscription key.
        /// </value>
        public string BingSpellCheckSubscriptionKey { get; set; }

        /// <summary>
        /// Gets or sets whether all intents come back or only the top one.
        /// </summary>
        /// <value>
        /// True for returning all intents.
        /// </value>
        public bool? IncludeAllIntents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not instance data should be included in response.
        /// </summary>
        /// <value>
        /// A value indicating whether or not instance data should be included in response.
        /// </value>
        public bool? IncludeInstanceData { get; set; }

        /// <summary>
        /// Gets or sets if queries should be logged in LUIS.
        /// </summary>
        /// <value>
        /// If queries should be logged in LUIS.
        /// </value>
        public bool? Log { get; set; }

        /// <summary>
        /// Gets or sets whether to spell check queries.
        /// </summary>
        /// <value>
        /// Whether to spell check queries.
        /// </value>
        public bool? SpellCheck { get; set; }

        /// <summary>
        /// Gets or sets whether to use the staging endpoint.
        /// </summary>
        /// <value>
        /// Whether to use the staging endpoint.
        /// </value>
        public bool? Staging { get; set; }

        /// <summary>
        /// Gets or sets the time in milliseconds to wait before the request times out.
        /// </summary>
        /// <value>
        /// The time in milliseconds to wait before the request times out. Default is 100000 milliseconds.
        /// </value>
        public double Timeout { get; set; } = 100000;

        /// <summary>
        /// Gets or sets the time zone offset.
        /// </summary>
        /// <value>
        /// The time zone offset.
        /// </value>
        public double? TimezoneOffset { get; set; }

        /// <summary>
        /// Gets or sets the IBotTelemetryClient used to log the LuisResult event.
        /// </summary>
        /// <value>
        /// The client used to log telemetry events.
        /// </value>
        [JsonIgnore]
        public IBotTelemetryClient TelemetryClient { get; set; } = new NullBotTelemetryClient();

        /// <summary>
        /// Gets or sets a value indicating whether to log personal information that came from the user to telemetry.
        /// </summary>
        /// <value>If true, personal information is logged to Telemetry; otherwise the properties will be filtered.</value>
        public bool LogPersonalInformation { get; set; } = false;
    }
}
