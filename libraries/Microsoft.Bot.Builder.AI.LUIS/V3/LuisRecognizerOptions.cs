// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.LuisV3
{
    /// <summary>
    /// Optional parameters for a LUIS recognizer.
    /// </summary>
     [Obsolete("This class has been deprecated, use Microsoft.Bot.Builder.AI.Luis.LuisRecognizerOptionsV2 or Microsoft.Bot.Builder.AI.Luis.LuisRecognizerOptionsV3 instead.")]
     public class LuisRecognizerOptions
    {
        /// <summary>
        /// Gets or sets the time span to wait before the request times out.
        /// </summary>
        /// <value>
        /// The time span to wait before the request times out. Default is 2 seconds.
        /// </value>
        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 2, 0);

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

        /// <summary>
        /// Gets or sets the handler for sending http calls.
        /// </summary>
        /// <value>
        /// Handler for intercepting http calls for logging or testing.
        /// </value>
        public HttpClientHandler HttpClient { get; set; }
    }
}
