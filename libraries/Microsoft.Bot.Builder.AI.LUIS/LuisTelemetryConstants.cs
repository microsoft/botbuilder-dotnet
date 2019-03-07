// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// The IBotTelemetryClient event and property names that logged by default.
    /// </summary>
    public static class LuisTelemetryConstants
    {
        public const string LuisResult = "LuisResult";  // Event name
        public const string ApplicationIdProperty = "applicationId";
        public const string IntentProperty = "intent";
        public const string IntentScoreProperty = "intentScore";
        public const string EntitiesProperty = "entities";
        public const string QuestionProperty = "question";
        public const string ActivityIdProperty = "activityId";
        public const string SentimentLabelProperty = "sentimentLabel";
        public const string SentimentScoreProperty = "sentimentScore";
        public const string FromIdProperty = "fromId";
    }
}
