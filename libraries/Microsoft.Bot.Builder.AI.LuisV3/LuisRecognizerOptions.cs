// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
using System.Net.Http;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Optional parameters for a LUIS recognizer.
    /// </summary>
     public class LuisRecognizerOptions
    {
        /// <summary>
        /// Gets or sets the time in milliseconds to wait before the request times out.
        /// </summary>
        /// <value>
        /// The time in milliseconds to wait before the request times out. Default is 100000 milliseconds.
        /// </value>
        public uint Timeout { get; set; } = 100000;

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
