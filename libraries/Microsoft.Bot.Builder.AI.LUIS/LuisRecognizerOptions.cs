// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Luis Recognizer Options.
    /// </summary>
    public abstract class LuisRecognizerOptions
    {
        protected LuisRecognizerOptions(LuisApplication application)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
        }

        /// <summary>
        /// Gets the LUIS application used to recognize text..
        /// </summary>
        /// <value>
        /// The LUIS application to use to recognize text.
        /// </value>
        public LuisApplication Application { get; private set; }

        /// <summary>
        /// Gets or sets the time in milliseconds to wait before the request times out.
        /// </summary>
        /// <value>
        /// The time in milliseconds to wait before the request times out. Default is 100000 milliseconds.
        /// </value>
        public double Timeout { get; set; } = 100000;

        /// <summary>
        /// Gets or sets the IBotTelemetryClient used to log the LuisResult event.
        /// </summary>
        /// <value>
        /// The client used to log telemetry events.
        /// </value>
        public IBotTelemetryClient TelemetryClient { get; set; } = new NullBotTelemetryClient();

        /// <summary>
        /// Gets or sets a value indicating whether to log personal information that came from the user to telemetry.
        /// </summary>
        /// <value>If true, personal information is logged to Telemetry; otherwise the properties will be filtered.</value>
        public bool LogPersonalInformation { get; set; } = false;

        public bool IncludeAPIResults { get; set; } = false;

        internal abstract Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext, HttpClient httpClient, CancellationToken cancellationToken);
    }
}
