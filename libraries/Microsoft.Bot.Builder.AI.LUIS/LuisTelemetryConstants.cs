// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// The IBotTelemetryClient event and property names that logged by default.
    /// </summary>
    public static class LuisTelemetryConstants
    {
        public static readonly string LuisResult = "LuisResult";  // Event name
        public static readonly string ApplicationIdProperty = "applicationId";
        public static readonly string IntentProperty = "intent";
        public static readonly string IntentScoreProperty = "intentScore";
        public static readonly string Intent2Property = "intent2";
        public static readonly string IntentScore2Property = "intentScore2";
        public static readonly string EntitiesProperty = "entities";
        public static readonly string QuestionProperty = "question";
        public static readonly string ActivityIdProperty = "activityId";
        public static readonly string SentimentLabelProperty = "sentimentLabel";
        public static readonly string SentimentScoreProperty = "sentimentScore";
        public static readonly string FromIdProperty = "fromId";
    }
}
