// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// The IBotTelemetryClient event and property names that logged by default.
    /// </summary>
    public static class LuisTelemetryConstants
    {
        /// <summary>
        /// The Key used when storing a LUIS Result in a custom event within telemetry.
        /// </summary>
        public static readonly string LuisResult = "LuisResult";  // Event name

        /// <summary>
        /// The Key used when storing a LUIS app ID in a custom event within telemetry.
        /// </summary>
        public static readonly string ApplicationIdProperty = "applicationId";

        /// <summary>
        /// The Key used when storing a LUIS intent in a custom event within telemetry.
        /// </summary>
        public static readonly string IntentProperty = "intent";

        /// <summary>
        /// The Key used when storing a LUIS intent score in a custom event within telemetry.
        /// </summary>
        public static readonly string IntentScoreProperty = "intentScore";

        /// <summary>
        /// The Key used when storing a LUIS intent in a custom event within telemetry.
        /// </summary>
        public static readonly string Intent2Property = "intent2";

        /// <summary>
        /// The Key used when storing a LUIS intent score in a custom event within telemetry.
        /// </summary>
        public static readonly string IntentScore2Property = "intentScore2";

        /// <summary>
        /// The Key used when storing LUIS entities in a custom event within telemetry.
        /// </summary>
        public static readonly string EntitiesProperty = "entities";

        /// <summary>
        /// The Key used when storing the LUIS query in a custom event within telemetry.
        /// </summary>
        public static readonly string QuestionProperty = "question";

        /// <summary>
        /// The Key used when storing an Activity ID in a custom event within telemetry.
        /// </summary>
        public static readonly string ActivityIdProperty = "activityId";

        /// <summary>
        /// The Key used when storing a sentiment label in a custom event within telemetry.
        /// </summary>
        public static readonly string SentimentLabelProperty = "sentimentLabel";

        /// <summary>
        /// The Key used when storing a LUIS sentiment score in a custom event within telemetry.
        /// </summary>
        public static readonly string SentimentScoreProperty = "sentimentScore";

        /// <summary>
        /// The Key used when storing the FromId in a custom event within telemetry.
        /// </summary>
        public static readonly string FromIdProperty = "fromId";
    }
}
