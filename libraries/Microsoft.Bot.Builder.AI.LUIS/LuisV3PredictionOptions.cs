// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Optional parameters for a LUIS prediction request.
    /// </summary>
    public class LuisV3PredictionOptions
    {
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
        /// Gets or sets the time in milliseconds to wait before the request times out.
        /// </summary>
        /// <value>
        /// The time in milliseconds to wait before the request times out. Default is 100000 milliseconds.
        /// </value>
        /// <remarks>
        /// This value can only be set when <see cref="LuisRecognizer"/> is created and can't be changed
        /// in individual <see cref="IRecognizer.RecognizeAsync"/> calls.
        /// </remarks>
        public double Timeout { get; set; } = 100000;

        /// <summary>
        /// Gets or sets the datetime used for resolving relative datetime references.
        /// </summary>
        /// <value>
        /// The datetime to use for resolving relative datetime references.
        /// </value>
        public DateTime? DatetimeReference { get; set; }

        /// <summary>
        /// Gets or sets dynamic lists used to recognize entities for a particular query.
        /// </summary>
        /// <value>
        /// Dynamic lists of things like contact names to recognize at query time.
        /// </value>
        public IList<DynamicList> DynamicLists { get; set; }

        /// <summary>
        /// Gets or sets external entities recognized in the query.
        /// </summary>
        /// <value>
        /// External entities recognized in query.
        /// </value>
        public IList<ExternalEntity> ExternalEntities { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if external entities should override other means of recognizing entities.
        /// </summary>
        /// <value>
        /// Boolean for if external entities should be preferred.
        /// </value>
        public bool? PreferExternalEntities { get; set; }

        /// <summary>
        /// Gets or sets a value for what deployment slot should be used for prediction.
        /// </summary>
        /// <value>
        /// LUIS slot name to access.
        /// </value>
        public string Slot { get; set; } = "production";

        /// <summary>
        /// Gets or sets the specific version of the model to access.
        /// </summary>
        /// <value>
        /// Version to access.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the IBotTelemetryClient used to log the LuisResult event.
        /// </summary>
        /// <value>
        /// The client used to log telemetry events.
        /// </value>
        /// <remarks>
        /// This value can only be set when <see cref="LuisRecognizer"/> is created and can't be changed
        /// in individual <see cref="IRecognizer.RecognizeAsync"/> calls.
        /// </remarks>
        [JsonIgnore]
        public IBotTelemetryClient TelemetryClient { get; set; } = new NullBotTelemetryClient();

        /// <summary>
        /// Gets or sets a value indicating whether to log personal information that came from the user to telemetry.
        /// </summary>
        /// <value>If true, personal information is logged to Telemetry; otherwise the properties will be filtered.</value>
        /// <remarks>
        /// This value can only be set when <see cref="LuisRecognizer"/> is created and can't be changed
        /// in individual <see cref="IRecognizer.RecognizeAsync"/> calls.
        /// </remarks>
        public bool LogPersonalInformation { get; set; } = false;
    }
}
